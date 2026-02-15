using Isekai.VSlice.Core.Content.Dto;
using Isekai.VSlice.Core.Runtime;

namespace Isekai.VSlice.Core.Systems;

public static class Resolver
{
    public static void StartOfTurn(BattleState s, ActorInstance a)
    {
        if (!a.IsAlive) return;

        // Apply DoTs first (Burning)
        var burn = a.GetStatus("st_burning");
        if (burn is not null)
        {
            int dmg = 5 * Math.Max(1, burn.Stacks);
            ApplyDamage(s, source: null, target: a, dmg, element: "fire", isDot: true);
        }
    }

    public static void EndOfTurn(BattleState s, ActorInstance a)
    {
        if (!a.IsAlive) return;

        for (int i = a.Statuses.Count - 1; i >= 0; i--)
        {
            var st = a.Statuses[i];
            if (st.Id == "st_exposed") continue; // sticky by design
            st.RemainingTurns -= 1;
            if (st.RemainingTurns <= 0)
                a.Statuses.RemoveAt(i);
        }
    }

    public static void ExecuteAbility(BattleState s, ActorInstance user, AbilityTemplateDto ab, ActorInstance target)
    {
        if (!user.IsAlive) return;
        if (!target.IsAlive) return;

        user.Mp -= ab.MpCost;
        if (user.Mp < 0) user.Mp = 0;

        // Guard/Brace clears Exposed
        if (ab.AbilityId is "ab_guard" or "ab_brace")
            user.RemoveStatus("st_exposed");

        switch (ab.Resolution.Type)
        {
            case "utility":
                ApplyStatuses(s, user, target, ab.Resolution.ApplyStatuses);
                if (ab.Resolution.Utility is not null)
                {
                    if (ab.Resolution.Utility.Kind == "free_step")
                    {
                        // v0: do nothing (movement handled by AI decision)
                    }
                    else if (ab.Resolution.Utility.Kind == "spawn_obstacle")
                    {
                        // v0: ignore (map is static in this slice)
                    }
                }
                break;

            case "cleanse":
                DoCleanse(s, user, target, ab);
                break;

            case "heal":
                DoHeal(s, user, target, ab);
                break;

            case "attack":
                DoAttack(s, user, target, ab);
                break;

            default:
                s.Log.Add($"[WARN] Unknown resolution type '{ab.Resolution.Type}' for {ab.AbilityId}");
                break;
        }
    }

    public static void ExecuteMove(BattleState s, ActorInstance a, int x, int y)
    {
        if (!a.IsAlive) return;
        if (!s.InBounds(x, y)) return;
        if (s.IsBlocked(x, y)) return;
        if (s.ActorAt(x, y) is not null) return;

        s.Log.Add($"{a.ColorTag} moves ({a.X},{a.Y}) -> ({x},{y})");
        a.X = x;
        a.Y = y;
    }

    private static void DoCleanse(BattleState s, ActorInstance user, ActorInstance target, AbilityTemplateDto ab)
    {
        int remove = ab.Resolution.Cleanse?.Remove ?? 1;
        if (remove <= 0) return;

        var disp = target.Statuses
            .Where(st => st.Template.Flags.Dispellable)
            .OrderByDescending(st => st.RemainingTurns)
            .Take(remove)
            .ToList();

        foreach (var st in disp)
        {
            target.Statuses.Remove(st);
            s.Log.Add($"{user.ColorTag} cleanses {st.Id} from {target.ColorTag}");
        }
    }

    private static void DoHeal(BattleState s, ActorInstance user, ActorInstance target, AbilityTemplateDto ab)
    {
        var h = ab.Resolution.Heal;
        if (h is null) return;

        int statVal = ReadStat(user, h.Stat);
        int amt = (int)Math.Round(h.Base + statVal * h.Scale);

        int before = target.Hp;
        target.Hp = Math.Min(target.Template.BaseStats.MaxHp, target.Hp + Math.Max(0, amt));

        s.Log.Add($"{user.ColorTag} uses {ab.AbilityId} on {target.ColorTag} HEAL {before}->{target.Hp}");
    }

    private static void DoAttack(BattleState s, ActorInstance user, ActorInstance target, AbilityTemplateDto ab)
    {
        // Hit check: base + accuracy_add (st_focused) + hit_bonus_vs_target (st_marked) - evasion
        double baseHit = 0.75;
        double accBonus = user.SumStatusEffect("accuracy_add");
        double targetHitBonus = target.SumStatusEffect("attacker_hit_bonus_vs_target");
        double hitChance = Clamp(baseHit + user.AccuracyMod - target.EvasionMod + accBonus + targetHitBonus, 0.05, 0.95);
        bool hit = s.Rng.NextDouble() < hitChance;

        if (!hit)
        {
            s.Log.Add($"{user.ColorTag} uses {ab.AbilityId} on {target.ColorTag} => MISS");
            return;
        }

        // Crit check: base + crit_bonus_vs_target (st_marked)
        double baseCrit = 0.05;
        double critBonus = target.SumStatusEffect("attacker_crit_bonus_vs_target");
        bool crit = s.Rng.NextDouble() < Clamp(baseCrit + critBonus, 0.0, 0.50);

        int dmg = ComputeDamage(s, user, target, ab);
        if (crit) dmg = (int)Math.Round(dmg * 1.5);

        if (dmg <= 0) dmg = 1;

        string hitType = crit ? "CRIT" : "HIT";
        ApplyDamage(s, user, target, dmg, element: ab.Resolution.Damage?.Elemental?.Element ?? "none", isDot: false, hitType: hitType);

        // Lifesteal
        if (ab.Resolution.Lifesteal is not null && dmg > 0)
        {
            int leeched = (int)Math.Round(dmg * ab.Resolution.Lifesteal.Fraction);
            int before = user.Hp;
            user.Hp = Math.Min(user.Template.BaseStats.MaxHp, user.Hp + Math.Max(0, leeched));
            s.Log.Add($"  lifesteal: {user.ColorTag} {before}->{user.Hp} (+{leeched})");
        }

        ApplyStatuses(s, user, target, ab.Resolution.ApplyStatuses);
    }

    private static int ComputeDamage(BattleState s, ActorInstance user, ActorInstance target, AbilityTemplateDto ab)
    {
        int total = 0;

        var dmg = ab.Resolution.Damage;
        if (dmg is null) return 0;

        if (dmg.Physical is not null)
        {
            int phys = PartDamage(user, dmg.Physical);
            phys = Math.Max(0, phys - target.Def);
            total += phys;
        }

        if (dmg.Elemental is not null)
        {
            int elem = PartDamage(user, dmg.Elemental);

            double elemMult = user.ProductStatusEffect("elemental_damage_mult");
            elem = (int)Math.Round(elem * elemMult);

            elem = Math.Max(0, elem - (target.Def / 2));
            total += elem;
        }

        // Data-driven incoming damage multiplier (Guarding 0.75, Exposed 1.2, etc.)
        double incomingMult = target.ProductStatusEffect("incoming_damage_mult");
        total = (int)Math.Round(total * incomingMult);

        return Math.Max(0, total);
    }

    private static int PartDamage(ActorInstance user, DamagePartDto part)
    {
        int statVal = ReadStat(user, part.Stat);
        double raw = part.Base + statVal * part.Scale;
        return Math.Max(0, (int)Math.Round(raw));
    }

    private static int ReadStat(ActorInstance user, string? stat)
    {
        return stat switch
        {
            "atk" => user.Atk,
            "def" => user.Def,
            "int" => user.Int,
            "wis" => user.Wis,
            "none" or null => 0,
            _ => 0
        };
    }

    private static void ApplyStatuses(BattleState s, ActorInstance user, ActorInstance target, List<ApplyStatusDto>? list)
    {
        if (list is null || list.Count == 0) return;

        foreach (var app in list)
        {
            if (!s.Content.StatusById.TryGetValue(app.StatusId, out var tpl))
            {
                s.Log.Add($"[WARN] Missing status template '{app.StatusId}'");
                continue;
            }

            var existing = target.GetStatus(app.StatusId);
            if (existing is null)
            {
                target.Statuses.Add(new StatusInstance
                {
                    Template = tpl,
                    Stacks = Math.Max(1, app.Stacks),
                    RemainingTurns = Math.Max(1, app.Duration)
                });
            }
            else
            {
                existing.Stacks = Math.Min(existing.Template.Stacking.Cap, Math.Max(existing.Stacks, app.Stacks));
                existing.RemainingTurns = Math.Max(existing.RemainingTurns, app.Duration);
            }

            s.Log.Add($"  status: {target.ColorTag} gains {app.StatusId} (dur={app.Duration})");
        }
    }

    private static void ApplyDamage(BattleState s, ActorInstance? source, ActorInstance target, int dmg, string element, bool isDot, string hitType = "HIT")
    {
        dmg = Math.Max(0, dmg);
        int before = target.Hp;
        target.Hp = Math.Max(0, target.Hp - dmg);

        if (source is null)
            s.Log.Add($"DOT {element}: {target.ColorTag} {before}->{target.Hp} (-{dmg})");
        else
            s.Log.Add($"{source.ColorTag} {hitType} {target.ColorTag} {before}->{target.Hp} (-{dmg}) via {element}");

        if (before > 0 && target.Hp == 0)
            s.Log.Add($"\x1b[97;41m*** KO: {target.ColorTag}\x1b[0m");
    }

    private static double Clamp(double v, double lo, double hi) => (v < lo) ? lo : (v > hi) ? hi : v;
}
