using Isekai.VSlice.Core.Content.Dto;

namespace Isekai.VSlice.Core.Content;

public static class ContentValidator
{
    public static void ValidateOrThrow(ContentPack pack)
    {
        var errors = Validate(pack);
        if (errors.Count > 0)
            throw new ContentException(errors);
    }

    public static List<string> Validate(ContentPack pack)
    {
        var errors = new List<string>();

        // Uniqueness
        CheckUnique(pack.ActorsPc, x => x.ActorTemplateId, "actors_pc.json", errors);
        CheckUnique(pack.ActorsEnemy, x => x.ActorTemplateId, "actors_enemy.json", errors);
        CheckUnique(pack.Abilities, x => x.AbilityId, "abilities.json", errors);
        CheckUnique(pack.Statuses, x => x.StatusId, "statuses.json", errors);
        CheckUnique(pack.Maps, x => x.MapId, "maps.json", errors);
        CheckUnique(pack.Encounters, x => x.EncounterTemplateId, "encounters.json", errors);
        CheckUnique(pack.Palettes, x => x.EnemyPaletteId, "palettes.json", errors);
        CheckUnique(pack.Rewards, x => x.RewardTableId, "rewards.json", errors);

        // Actor -> ability references
        ValidateActorAbilities(pack.ActorsPc, "actors_pc.json", pack.AbilityById, errors);
        ValidateActorAbilities(pack.ActorsEnemy, "actors_enemy.json", pack.AbilityById, errors);

        // Ability -> status references
        foreach (var ab in pack.Abilities)
        {
            bool isFreeAction = string.Equals(ab.Resolution.Utility?.Kind, "free_step", StringComparison.OrdinalIgnoreCase);

            if (isFreeAction)
            {
                if (ab.CtCost != 0)
                    errors.Add($"abilities.json: free/cantrip ability '{ab.AbilityId}' must have ct_cost == 0.");
            }
            else if (ab.CtCost <= 0)
            {
                errors.Add($"abilities.json: primary ability '{ab.AbilityId}' must have ct_cost >= 1.");
            }

            if (ab.Resolution.Utility?.Kind?.Contains("refund", StringComparison.OrdinalIgnoreCase) == true)
                errors.Add($"abilities.json: ability '{ab.AbilityId}' includes prohibited immediate CT refund utility '{ab.Resolution.Utility.Kind}'.");

            var list = ab.Resolution.ApplyStatuses;
            if (list is null) continue;
            foreach (var s in list)
            {
                if (!pack.StatusById.ContainsKey(s.StatusId))
                    errors.Add($"abilities.json: ability '{ab.AbilityId}' references missing status '{s.StatusId}'.");
            }
        }

        // Encounter references
        foreach (var enc in pack.Encounters)
        {
            if (!pack.MapById.ContainsKey(enc.MapId))
                errors.Add($"encounters.json: encounter '{enc.EncounterTemplateId}' references missing map '{enc.MapId}'.");

            if (!pack.PaletteById.ContainsKey(enc.EnemyPaletteId))
                errors.Add($"encounters.json: encounter '{enc.EncounterTemplateId}' references missing palette '{enc.EnemyPaletteId}'.");
        }

        // Palette entries reference enemies
        foreach (var pal in pack.Palettes)
        {
            foreach (var e in pal.Entries)
            {
                if (!pack.EnemyById.ContainsKey(e.EnemyTemplateId))
                    errors.Add($"palettes.json: palette '{pal.EnemyPaletteId}' references missing enemy '{e.EnemyTemplateId}'.");
                if (e.Weight <= 0)
                    errors.Add($"palettes.json: palette '{pal.EnemyPaletteId}' has non-positive weight for '{e.EnemyTemplateId}'.");
            }
        }

        // Map bounds & tile sanity
        foreach (var map in pack.Maps)
        {
            if (map.Size.W <= 0 || map.Size.H <= 0)
                errors.Add($"maps.json: map '{map.MapId}' has invalid size {map.Size.W}x{map.Size.H}.");

            // tile coords in bounds + no duplicates
            var seen = new HashSet<(int x, int y)>();
            foreach (var t in map.Tiles)
            {
                if (t.X < 0 || t.Y < 0 || t.X >= map.Size.W || t.Y >= map.Size.H)
                    errors.Add($"maps.json: map '{map.MapId}' tile out of bounds at ({t.X},{t.Y}).");

                if (!seen.Add((t.X, t.Y)))
                    errors.Add($"maps.json: map '{map.MapId}' duplicate tile at ({t.X},{t.Y}).");
            }

            // spawn zones in bounds
            foreach (var (zoneName, points) in map.SpawnZones)
            {
                if (points.Count == 0)
                    errors.Add($"maps.json: map '{map.MapId}' spawn zone '{zoneName}' is empty.");

                foreach (var p in points)
                {
                    if (p.X < 0 || p.Y < 0 || p.X >= map.Size.W || p.Y >= map.Size.H)
                        errors.Add($"maps.json: map '{map.MapId}' spawn zone '{zoneName}' out of bounds at ({p.X},{p.Y}).");
                }
            }
        }

        // Faction sanity (light check)
        foreach (var pc in pack.ActorsPc)
            if (!string.Equals(pc.Faction, "player", StringComparison.OrdinalIgnoreCase))
                errors.Add($"actors_pc.json: '{pc.ActorTemplateId}' faction should be 'player' but is '{pc.Faction}'.");

        foreach (var en in pack.ActorsEnemy)
            if (!string.Equals(en.Faction, "enemy", StringComparison.OrdinalIgnoreCase))
                errors.Add($"actors_enemy.json: '{en.ActorTemplateId}' faction should be 'enemy' but is '{en.Faction}'.");

        return errors;
    }

    private static void ValidateActorAbilities(
        IEnumerable<ActorTemplateDto> actors,
        string fileName,
        IReadOnlyDictionary<string, AbilityTemplateDto> abilityById,
        List<string> errors)
    {
        foreach (var a in actors)
        {
            foreach (var abId in a.Abilities)
            {
                if (!abilityById.ContainsKey(abId))
                    errors.Add($"{fileName}: actor '{a.ActorTemplateId}' references missing ability '{abId}'.");
            }
        }
    }

    private static void CheckUnique<T, TKey>(
        IEnumerable<T> items,
        Func<T, TKey> keySelector,
        string fileName,
        List<string> errors)
        where TKey : notnull
    {
        var seen = new HashSet<TKey>();
        foreach (var item in items)
        {
            var key = keySelector(item);
            if (!seen.Add(key))
                errors.Add($"{fileName}: duplicate id '{key}'.");
        }
    }
}
