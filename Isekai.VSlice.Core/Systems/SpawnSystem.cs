using Isekai.VSlice.Core.Content;
using Isekai.VSlice.Core.Content.Dto;
using Isekai.VSlice.Core.Runtime;

namespace Isekai.VSlice.Core.Systems;

public static class SpawnSystem
{
    private static int CostForTier(int tier) => Math.Max(1, tier) * 3;

    private const int ENEMY_TARGET_UNITS = 6;
    private const int ENEMY_MAX_UNITS = 8;

    public static BattleState CreateBattle(ContentPack pack, EncounterTemplateDto encounter, int seed = 12345)
    {
        var map = pack.MapById[encounter.MapId];
        var pal = pack.PaletteById[encounter.EnemyPaletteId];

        var rng = new Random(seed);
        var log = new BattleLog();
        var actors = new List<ActorInstance>();

        // -----------------
        // Spawn PCs (zone_A)
        // -----------------
        var pcSpots = map.SpawnZones[encounter.SpawnRules.PlayerSpawn];
        for (int i = 0; i < pack.ActorsPc.Count; i++)
        {
            var spot = pcSpots[i % pcSpots.Count];
            var tpl = pack.ActorsPc[i];

            actors.Add(new ActorInstance
            {
                InstanceId = $"pc_{i}",
                Template = tpl,
                X = spot.X,
                Y = spot.Y,
                Hp = tpl.BaseStats.MaxHp,
                Mp = tpl.BaseStats.MaxMp,
                Ct = 0
            });
        }

        // -----------------------
        // Spawn Enemies (zone_B)
        // -----------------------
        var enSpots = map.SpawnZones[encounter.SpawnRules.EnemySpawn].ToList();
        ShuffleInPlace(enSpots, rng);

        int budgetLeft = encounter.Budget;
        int unitCapLeft = encounter.UnitCap;

        int desiredUnits = PickDesiredEnemyCount(encounter.EncounterTemplateId, rng);
        desiredUnits = Math.Min(desiredUnits, ENEMY_MAX_UNITS);
        desiredUnits = Math.Min(desiredUnits, unitCapLeft);
        desiredUnits = Math.Min(desiredUnits, enSpots.Count);

        int spawned = 0;

        // Composition tracking (data-driven from encounter.Composition)
        var comp = encounter.Composition;
        int maxSameTemplate = comp?.MaxSameTemplate ?? int.MaxValue;
        var maxRoleTagCounts = comp?.MaxRoleTagCounts;

        var templateCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        var roleTagCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var unsatisfiedRoles = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (comp?.MinRoleTags is not null)
        {
            foreach (var kv in comp.MinRoleTags)
                unsatisfiedRoles[kv.Key] = kv.Value;
        }

        while (spawned < desiredUnits && budgetLeft > 0 && unitCapLeft > 0 && spawned < enSpots.Count)
        {
            int slotsRemaining = Math.Max(1, desiredUnits - spawned);

            ActorTemplateDto? chosen = null;

            // Phase 1: Try to satisfy unsatisfied composition minimums
            foreach (var role in unsatisfiedRoles.Keys.ToList())
            {
                if (unsatisfiedRoles[role] <= 0) continue;

                chosen = TryPickByPredicate(pack, pal.Entries, rng,
                    tpl => tpl.RoleTags != null && tpl.RoleTags.Contains(role),
                    budgetLeft, templateCounts, maxSameTemplate, maxRoleTagCounts, roleTagCounts);

                if (chosen != null)
                {
                    if (chosen.RoleTags != null)
                    {
                        foreach (var tag in chosen.RoleTags)
                        {
                            if (unsatisfiedRoles.ContainsKey(tag))
                                unsatisfiedRoles[tag] = Math.Max(0, unsatisfiedRoles[tag] - 1);
                        }
                    }
                    break;
                }
            }

            // Phase 2: General weighted pick with tier bias and composition constraints
            if (chosen == null)
            {
                var chosenId = WeightedPickWithTierBias(pack, pal.Entries, rng, budgetLeft, slotsRemaining,
                    templateCounts, maxSameTemplate, maxRoleTagCounts, roleTagCounts);

                if (chosenId != null)
                    chosen = pack.EnemyById[chosenId];
            }

            if (chosen == null) break;

            int cost = CostForTier(chosen.Tier);

            if (cost > budgetLeft)
            {
                bool found = false;
                for (int tries = 0; tries < 12; tries++)
                {
                    var id2 = WeightedPick(pal.Entries, rng);
                    var t2 = pack.EnemyById[id2];
                    var c2 = CostForTier(t2.Tier);
                    if (c2 <= budgetLeft && IsAllowedByComposition(t2, templateCounts, maxSameTemplate, maxRoleTagCounts, roleTagCounts))
                    {
                        chosen = t2;
                        cost = c2;
                        found = true;
                        break;
                    }
                }
                if (!found) break;
            }

            var spot = enSpots[spawned];

            actors.Add(new ActorInstance
            {
                InstanceId = $"en_{spawned}",
                Template = chosen,
                X = spot.X,
                Y = spot.Y,
                Hp = chosen.BaseStats.MaxHp,
                Mp = chosen.BaseStats.MaxMp,
                Ct = 0
            });

            templateCounts[chosen.ActorTemplateId] = templateCounts.GetValueOrDefault(chosen.ActorTemplateId, 0) + 1;
            if (chosen.RoleTags != null)
            {
                foreach (var tag in chosen.RoleTags)
                    roleTagCounts[tag] = roleTagCounts.GetValueOrDefault(tag, 0) + 1;
            }

            spawned++;
            unitCapLeft--;
            budgetLeft -= cost;
        }

        // Spawn summary (with color)
        log.AddHeader("SPAWN");
        log.Add($"Encounter: {encounter.EncounterTemplateId}  Map: {map.MapId}  Seed: {seed}");
        log.Add($"Budget: {encounter.Budget}  UnitCap: {encounter.UnitCap}  DesiredEnemies: {desiredUnits}  Spawned: {spawned}  BudgetLeft: {budgetLeft}");
        if (maxSameTemplate < int.MaxValue)
            log.Add($"MaxSameTemplate: {maxSameTemplate}");
        foreach (var a in actors)
            log.Add($"{a.ColorTag,-36} HP={a.Hp} MP={a.Mp} SPD={a.Speed} @({a.X},{a.Y})");

        return new BattleState
        {
            Content = pack,
            Map = map,
            Actors = actors,
            Rng = rng,
            Log = log
        };
    }

    private static bool IsAllowedByComposition(
        ActorTemplateDto candidate,
        Dictionary<string, int> templateCounts,
        int maxSameTemplate,
        Dictionary<string, int>? maxRoleTagCounts,
        Dictionary<string, int> roleTagCounts)
    {
        int currentTemplateCount = templateCounts.GetValueOrDefault(candidate.ActorTemplateId, 0);
        if (currentTemplateCount >= maxSameTemplate)
            return false;

        if (maxRoleTagCounts is not null && candidate.RoleTags is not null)
        {
            foreach (var tag in candidate.RoleTags)
            {
                if (maxRoleTagCounts.TryGetValue(tag, out int maxCount))
                {
                    int currentTagCount = roleTagCounts.GetValueOrDefault(tag, 0);
                    if (currentTagCount >= maxCount)
                        return false;
                }
            }
        }

        return true;
    }

    private static int PickDesiredEnemyCount(string encounterId, Random rng)
    {
        var id = (encounterId ?? "").ToLowerInvariant();

        if (id.Contains("pilot"))
            return PickDesiredEnemyCount_Pilot(rng);
        if (id.Contains("easy"))
            return PickDesiredEnemyCount_Easy(rng);
        if (id.Contains("hard"))
            return PickDesiredEnemyCount_Hard(rng);
        return PickDesiredEnemyCount_Normal(rng);
    }

    private static int PickDesiredEnemyCount_Easy(Random rng)
    {
        int roll = rng.Next(1, 101);
        if (roll <= 30) return 4;
        if (roll <= 80) return 5;
        return 6;
    }

    private static int PickDesiredEnemyCount_Normal(Random rng)
    {
        int roll = rng.Next(1, 101);
        if (roll <= 10) return 4;
        if (roll <= 25) return 5;
        if (roll <= 70) return 6;
        if (roll <= 90) return 7;
        return 8;
    }

    private static int PickDesiredEnemyCount_Hard(Random rng)
    {
        int roll = rng.Next(1, 101);
        if (roll <= 20) return 6;
        if (roll <= 75) return 7;
        return 8;
    }

    private static int PickDesiredEnemyCount_Pilot(Random rng)
    {
        int roll = rng.Next(1, 101);
        if (roll <= 2) return 1;
        if (roll <= 5) return 2;
        if (roll <= 12) return 3;
        if (roll <= 22) return 4;
        if (roll <= 38) return 5;
        if (roll <= 62) return 6;
        if (roll <= 78) return 7;
        if (roll <= 90) return 8;
        if (roll <= 97) return 9;
        return 10;
    }

    private static ActorTemplateDto? TryPickByPredicate(
        ContentPack pack,
        List<EnemyPaletteEntryDto> entries,
        Random rng,
        Func<ActorTemplateDto, bool> predicate,
        int budgetLeft,
        Dictionary<string, int> templateCounts,
        int maxSameTemplate,
        Dictionary<string, int>? maxRoleTagCounts,
        Dictionary<string, int> roleTagCounts)
    {
        var filtered = entries
            .Where(e =>
            {
                if (!pack.EnemyById.TryGetValue(e.EnemyTemplateId, out var tpl)) return false;
                if (!predicate(tpl)) return false;
                if (CostForTier(tpl.Tier) > budgetLeft) return false;
                if (!IsAllowedByComposition(tpl, templateCounts, maxSameTemplate, maxRoleTagCounts, roleTagCounts)) return false;
                return true;
            })
            .ToList();

        if (filtered.Count == 0) return null;

        var id = WeightedPick(filtered, rng);
        return pack.EnemyById[id];
    }

    private static string? WeightedPickWithTierBias(
        ContentPack pack,
        List<EnemyPaletteEntryDto> entries,
        Random rng,
        int budgetLeft,
        int slotsRemaining,
        Dictionary<string, int> templateCounts,
        int maxSameTemplate,
        Dictionary<string, int>? maxRoleTagCounts,
        Dictionary<string, int> roleTagCounts)
    {
        int denom = Math.Max(1, slotsRemaining);
        int targetSpendPerSlot = budgetLeft / denom;

        var adjusted = new List<(string id, int weight)>();
        foreach (var e in entries)
        {
            if (!pack.EnemyById.TryGetValue(e.EnemyTemplateId, out var tpl)) continue;

            int cost = CostForTier(tpl.Tier);
            int baseW = Math.Max(0, e.Weight);

            if (cost > budgetLeft || !IsAllowedByComposition(tpl, templateCounts, maxSameTemplate, maxRoleTagCounts, roleTagCounts))
            {
                adjusted.Add((e.EnemyTemplateId, 0));
                continue;
            }

            int diff = Math.Abs(cost - targetSpendPerSlot);

            int multPct =
                diff <= 2 ? 150 :
                diff <= 5 ? 120 :
                diff <= 10 ? 100 :
                67;

            int newW = (baseW * multPct) / 100;
            adjusted.Add((e.EnemyTemplateId, Math.Max(0, newW)));
        }

        if (adjusted.Count == 0 || adjusted.All(a => a.weight <= 0))
            return null;

        int total = adjusted.Sum(a => Math.Max(0, a.weight));
        int roll = rng.Next(1, total + 1);
        int acc = 0;
        foreach (var a in adjusted)
        {
            acc += Math.Max(0, a.weight);
            if (roll <= acc) return a.id;
        }
        return adjusted[^1].id;
    }

    private static string WeightedPick(List<EnemyPaletteEntryDto> entries, Random rng)
    {
        int total = entries.Sum(e => Math.Max(0, e.Weight));
        if (total <= 0) return entries[0].EnemyTemplateId;

        int roll = rng.Next(1, total + 1);
        int acc = 0;
        foreach (var e in entries)
        {
            acc += Math.Max(0, e.Weight);
            if (roll <= acc) return e.EnemyTemplateId;
        }
        return entries[^1].EnemyTemplateId;
    }

    private static void ShuffleInPlace<T>(IList<T> list, Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
