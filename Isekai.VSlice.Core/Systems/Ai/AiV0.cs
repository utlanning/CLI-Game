using Isekai.VSlice.Core.Content.Dto;
using Isekai.VSlice.Core.Runtime;

namespace Isekai.VSlice.Core.Systems.Ai;

public static class AiV0
{
    // --- Decision now supports combined Move + Act in one turn ---
    public sealed record Decision(string? AbilityId, ActorInstance? Target, int? MoveX, int? MoveY)
    {
        public bool HasMove => MoveX.HasValue && MoveY.HasValue;
        public bool HasAbility => AbilityId is not null && Target is not null;

        public static Decision Wait() => new(null, null, null, null);
        public static Decision MoveOnly(int x, int y) => new(null, null, x, y);
        public static Decision ActOnly(string abilityId, ActorInstance target) => new(abilityId, target, null, null);
        public static Decision MoveAndAct(int x, int y, string abilityId, ActorInstance target) => new(abilityId, target, x, y);
    }

    public const int MoveRange = 4;
    private const int ThreatRange = 3;

    // Heal triage thresholds (HP percentage at or below which healing is prioritized)
    private const double HealThresholdSupport = 0.75;
    private const double HealThresholdGeneral = 0.50;

    private static readonly HashSet<string> CleansableNegativeStatusIds =
        new(StringComparer.Ordinal)
        {
            "st_slowed",
            "st_weakened",
            "st_sundered",
            "st_blinded",
            "st_silenced",
            "st_cursed",
        };

    // ===================================================================
    //  MAIN ENTRY POINT
    // ===================================================================

    public static Decision ChooseAction(BattleState s, ActorInstance self)
    {
        if (!self.IsAlive) return Decision.Wait();

        var enemies = (self.Faction == "player")
            ? s.AliveEnemies.ToList()
            : s.AlivePlayers.ToList();

        if (enemies.Count == 0) return Decision.Wait();

        var allies = s.AliveActors
            .Where(a => a.Faction.Equals(self.Faction, StringComparison.OrdinalIgnoreCase))
            .ToList();

        bool threatened = IsThreatened(s, self, enemies);

        // Skirmish contact-window reset
        if (!threatened)
            self.UsedSkirmishThisContact = false;

        // -------------------------------------------------
        // 1) HEAL TRIAGE — prioritize healing injured allies
        // -------------------------------------------------
        var healDecision = TryHealTriage(s, self, allies, enemies);
        if (healDecision is not null) return healDecision;

        // -------------------------------------------------
        // 2) OFFENSIVE / UTILITY from current position
        // -------------------------------------------------
        var actDecision = TryOffensiveAbilities(s, self, enemies, threatened);
        if (actDecision is not null) return actDecision;

        // -------------------------------------------------
        // 3) MOVE toward nearest enemy, then re-try offense
        //    (This is the Move+Act combined turn)
        // -------------------------------------------------
        var moveActDecision = TryMoveAndAct(s, self, enemies, threatened);
        if (moveActDecision is not null) return moveActDecision;

        // -------------------------------------------------
        // 4) GUARD / BRACE fallback (only when threatened)
        // -------------------------------------------------
        if (threatened)
        {
            var guardDecision = TryGuard(s, self, enemies);
            if (guardDecision is not null) return guardDecision;
        }

        // -------------------------------------------------
        // 5) WAIT
        // -------------------------------------------------
        return Decision.Wait();
    }

    // ===================================================================
    //  1) HEAL TRIAGE
    // ===================================================================

    private static Decision? TryHealTriage(
        BattleState s, ActorInstance self,
        List<ActorInstance> allies, List<ActorInstance> enemies)
    {
        // Collect usable heal abilities
        var heals = new List<AbilityTemplateDto>();
        foreach (var abId in self.Template.Abilities)
        {
            if (!s.Content.AbilityById.TryGetValue(abId, out var ab)) continue;
            if (!string.Equals(ab.Resolution.Type, "heal", StringComparison.OrdinalIgnoreCase)) continue;
            if (self.Mp < ab.MpCost) continue;
            heals.Add(ab);
        }

        if (heals.Count == 0) return null;

        // Determine threshold based on role tags
        bool isSupport = self.Template.RoleTags?.Contains("support") ?? false;
        double threshold = isSupport ? HealThresholdSupport : HealThresholdGeneral;

        // Find hurt allies sorted by HP% ascending (most injured first)
        var hurtAllies = allies
            .Where(a => a.IsAlive && (double)a.Hp / a.Template.BaseStats.MaxHp <= threshold)
            .OrderBy(a => (double)a.Hp / a.Template.BaseStats.MaxHp)
            .ThenBy(a => a.InstanceId, StringComparer.Ordinal)
            .ToList();

        if (hurtAllies.Count == 0) return null;

        // Try healing from current position
        foreach (var heal in heals)
        {
            foreach (var ally in hurtAllies)
            {
                if (IsHealAtFullHp(heal, ally)) continue;
                if (CanTargetWith(s, self, heal, ally))
                    return Decision.ActOnly(heal.AbilityId, ally);
            }
        }

        // Try moving toward most injured ally, then healing
        var primaryTarget = hurtAllies[0];
        var (mx, my) = PathToward(s, self, primaryTarget.X, primaryTarget.Y, MoveRange);
        if (mx != self.X || my != self.Y)
        {
            Decision? result = null;
            foreach (var heal in heals)
            {
                foreach (var ally in hurtAllies)
                {
                    if (IsHealAtFullHp(heal, ally)) continue;
                    if (CanTargetWith(s, mx, my, heal, ally))
                    {
                        result = Decision.MoveAndAct(mx, my, heal.AbilityId, ally);
                        break;
                    }
                }
                if (result is not null) break;
            }
            if (result is not null) return result;
        }

        return null;
    }

    // ===================================================================
    //  2) OFFENSIVE / UTILITY from current position
    // ===================================================================

    private static Decision? TryOffensiveAbilities(
        BattleState s, ActorInstance self,
        List<ActorInstance> enemies, bool threatened)
    {
        bool hasOffenseNow = HasAnyOffensiveActionNow(s, self, enemies);

        foreach (var abId in self.Template.Abilities)
        {
            if (IsPureDefense(abId)) continue;
            if (!s.Content.AbilityById.TryGetValue(abId, out var ab)) continue;
            if (self.Mp < ab.MpCost) continue;

            // Skip heals — handled by triage (step 1)
            if (string.Equals(ab.Resolution.Type, "heal", StringComparison.OrdinalIgnoreCase)) continue;

            // Existing gates (unchanged)
            if (string.Equals(ab.AbilityId, "ab_cleanse", StringComparison.Ordinal) && !HasCleansableNegative(self))
                continue;

            if (IsFreeStep(ab) && !threatened) continue;
            if (IsFreeStep(ab) && threatened && self.UsedSkirmishThisContact) continue;

            if (hasOffenseNow && IsSelfBuffOrUtility(ab)) continue;

            if (string.Equals(ab.Targeting.Mode, "self", StringComparison.OrdinalIgnoreCase))
            {
                if (IsRedundantSelfBuff(ab, self)) continue;
                if (IsPointlessFreeStep(s, self, ab, enemies, threatened)) continue;
            }

            var (ok, target) = FindTargetForAbility(s, self, ab, enemies);
            if (ok && target is not null)
            {
                if (IsFreeStep(ab) && threatened)
                    self.UsedSkirmishThisContact = true;

                return Decision.ActOnly(ab.AbilityId, target);
            }
        }

        return null;
    }

    // ===================================================================
    //  3) MOVE + ACT (combined turn)
    // ===================================================================

    private static Decision? TryMoveAndAct(
        BattleState s, ActorInstance self,
        List<ActorInstance> enemies, bool threatened)
    {
        // Pick move target: nearest enemy
        var nearest = enemies
            .OrderBy(e => s.Manhattan(self, e))
            .ThenBy(e => e.InstanceId, StringComparer.Ordinal)
            .First();

        var (mx, my) = PathToward(s, self, nearest.X, nearest.Y, MoveRange);

        // If we can't move at all, nothing to do here
        if (mx == self.X && my == self.Y) return null;

        Decision? result = null;
        bool hasOffenseAfterMove = HasAnyOffensiveActionNow(s, self, enemies, mx, my);

        foreach (var abId in self.Template.Abilities)
        {
            if (IsPureDefense(abId)) continue;
            if (!s.Content.AbilityById.TryGetValue(abId, out var ab)) continue;
            if (self.Mp < ab.MpCost) continue;

            // Skip heals (handled by triage) and free-step (don't step after a full move)
            if (string.Equals(ab.Resolution.Type, "heal", StringComparison.OrdinalIgnoreCase)) continue;
            if (IsFreeStep(ab)) continue;

            if (string.Equals(ab.AbilityId, "ab_cleanse", StringComparison.Ordinal) && !HasCleansableNegative(self))
                continue;

            if (hasOffenseAfterMove && IsSelfBuffOrUtility(ab)) continue;

            if (string.Equals(ab.Targeting.Mode, "self", StringComparison.OrdinalIgnoreCase))
            {
                if (IsRedundantSelfBuff(ab, self)) continue;
            }

            var (ok, target) = FindTargetForAbility(s, self, ab, enemies, mx, my);
            if (ok && target is not null)
            {
                result = Decision.MoveAndAct(mx, my, ab.AbilityId, target);
                break;
            }
        }

        // If we found an ability to use after moving, return move+act
        if (result is not null) return result;

        // Otherwise, just move (still better than standing still)
        return Decision.MoveOnly(mx, my);
    }

    // ===================================================================
    //  4) GUARD / BRACE fallback
    // ===================================================================

    private static Decision? TryGuard(
        BattleState s, ActorInstance self,
        List<ActorInstance> enemies)
    {
        // Enemy rule: if enemy has offense now, never guard
        bool isEnemy = self.Faction.Equals("enemy", StringComparison.OrdinalIgnoreCase);
        if (isEnemy && HasAnyOffensiveActionNow(s, self, enemies))
            return null;

        foreach (var abId in self.Template.Abilities)
        {
            if (!IsPureDefense(abId)) continue;
            if (!s.Content.AbilityById.TryGetValue(abId, out var ab)) continue;
            if (self.Mp < ab.MpCost) continue;

            var (ok, target) = FindTargetForAbility(s, self, ab, enemies);
            if (ok && target is not null)
                return Decision.ActOnly(ab.AbilityId, target);
        }

        return null;
    }

    // ===================================================================
    //  TARGETING HELPERS
    // ===================================================================

    /// <summary>
    /// Checks if a specific actor can be targeted by an ability from current position.
    /// Used by heal triage to test specific allies.
    /// </summary>
    private static bool CanTargetWith(BattleState s, ActorInstance self, AbilityTemplateDto ab, ActorInstance target)
        => CanTargetWith(s, self.X, self.Y, ab, target);

    private static bool CanTargetWith(BattleState s, int originX, int originY, AbilityTemplateDto ab, ActorInstance target)
    {
        if (!target.IsAlive) return false;
        int dist = s.Manhattan(originX, originY, target.X, target.Y);
        if (dist > ab.Targeting.Range) return false;
        if (ab.Targeting.RequiresLos && !Systems.Los.HasLineOfSight(s, originX, originY, target.X, target.Y))
            return false;
        return true;
    }

    /// <summary>
    /// General target finder (unchanged from original logic).
    /// For enemy-targeting: picks nearest enemy in range.
    /// For ally-targeting: picks nearest ally in range (skips full-HP heals).
    /// For self-targeting: returns self (skips full-HP self-heals).
    /// </summary>
    private static (bool ok, ActorInstance? target) FindTargetForAbility(
        BattleState s,
        ActorInstance self,
        AbilityTemplateDto ab,
        List<ActorInstance> enemies,
        int? originX = null,
        int? originY = null)
    {
        int ox = originX ?? self.X;
        int oy = originY ?? self.Y;

        if (string.Equals(ab.Targeting.Mode, "self", StringComparison.OrdinalIgnoreCase))
        {
            if (IsHealAtFullHp(ab, self))
                return (false, null);
            return (true, self);
        }

        bool wantsAlly = ab.Targeting.Mode.Contains("ally", StringComparison.OrdinalIgnoreCase);
        var candidates = wantsAlly
            ? s.AliveActors.Where(a => a.IsAlive && a.Faction == self.Faction).ToList()
            : enemies;

        foreach (var c in candidates
                     .OrderBy(c => s.Manhattan(ox, oy, c.X, c.Y))
                     .ThenBy(c => c.InstanceId, StringComparer.Ordinal))
        {
            int dist = s.Manhattan(ox, oy, c.X, c.Y);
            if (dist > ab.Targeting.Range) continue;

            if (ab.Targeting.RequiresLos && !Systems.Los.HasLineOfSight(s, ox, oy, c.X, c.Y))
                continue;

            if (wantsAlly && IsHealAtFullHp(ab, c))
                continue;

            return (true, c);
        }

        return (false, null);
    }

    // ===================================================================
    //  PATHFINDING
    // ===================================================================

    private static (int x, int y) PathToward(BattleState s, ActorInstance self, int goalX, int goalY, int maxSteps)
    {
        var start = (self.X, self.Y);
        var q = new Queue<(int x, int y)>();
        var dist = new Dictionary<(int x, int y), int>();

        q.Enqueue(start);
        dist[start] = 0;

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            int d = dist[cur];
            if (d == maxSteps) continue;

            foreach (var n in s.Neighbors4(cur.x, cur.y))
            {
                if (!s.InBounds(n.x, n.y)) continue;
                if (s.IsBlocked(n.x, n.y)) continue;

                var occ = s.ActorAt(n.x, n.y);
                if (occ is not null && occ != self) continue;

                if (dist.ContainsKey((n.x, n.y))) continue;

                dist[(n.x, n.y)] = d + 1;
                q.Enqueue((n.x, n.y));
            }
        }

        var best = start;
        int bestScore = s.Manhattan(start.Item1, start.Item2, goalX, goalY);

        foreach (var kv in dist)
        {
            var tile = kv.Key;
            int steps = kv.Value;
            if (steps > maxSteps) continue;

            int score = s.Manhattan(tile.x, tile.y, goalX, goalY);
            if (score < bestScore)
            {
                best = tile;
                bestScore = score;
            }
        }

        return best;
    }

    // ===================================================================
    //  CLASSIFICATION HELPERS
    // ===================================================================

    private static bool IsThreatened(BattleState s, ActorInstance self, List<ActorInstance> enemies)
        => enemies.Any(e => e.IsAlive && s.Manhattan(self, e) <= ThreatRange);

    private static bool IsPureDefense(string abilityId)
        => abilityId is "ab_guard" or "ab_brace";

    private static bool IsFreeStep(AbilityTemplateDto ab)
        => ab.Resolution.Utility is not null
           && string.Equals(ab.Resolution.Utility.Kind, "free_step", StringComparison.OrdinalIgnoreCase);

    private static bool IsSelfBuffOrUtility(AbilityTemplateDto ab)
    {
        if (!string.Equals(ab.Targeting.Mode, "self", StringComparison.OrdinalIgnoreCase))
            return false;

        bool appliesStatus = ab.Resolution.ApplyStatuses is { Count: > 0 };
        bool isUtility = ab.Resolution.Utility is not null;
        bool isHeal = string.Equals(ab.Resolution.Type, "heal", StringComparison.OrdinalIgnoreCase);

        return !isHeal && (appliesStatus || isUtility);
    }

    private static bool HasAnyOffensiveActionNow(BattleState s, ActorInstance self, List<ActorInstance> enemies, int? originX = null, int? originY = null)
    {
        foreach (var abId in self.Template.Abilities)
        {
            if (IsPureDefense(abId)) continue;
            if (!s.Content.AbilityById.TryGetValue(abId, out var ab)) continue;
            if (self.Mp < ab.MpCost) continue;

            bool isOffensive =
                string.Equals(ab.Resolution.Type, "attack", StringComparison.OrdinalIgnoreCase) ||
                (ab.Resolution.ApplyStatuses is { Count: > 0 } && !string.Equals(ab.Resolution.Type, "heal", StringComparison.OrdinalIgnoreCase));

            if (!isOffensive) continue;

            var (ok, tgt) = FindTargetForAbility(s, self, ab, enemies, originX, originY);
            if (ok && tgt is not null && tgt != self)
                return true;
        }

        return false;
    }

    private static bool HasCleansableNegative(ActorInstance a)
    {
        foreach (var st in a.Statuses)
            if (CleansableNegativeStatusIds.Contains(st.Id))
                return true;
        return false;
    }

    private static bool IsHealAtFullHp(AbilityTemplateDto ab, ActorInstance target)
    {
        if (!string.Equals(ab.Resolution.Type, "heal", StringComparison.OrdinalIgnoreCase))
            return false;
        return target.Hp >= target.Template.BaseStats.MaxHp;
    }

    private static bool IsRedundantSelfBuff(AbilityTemplateDto ab, ActorInstance self)
    {
        if (!string.Equals(ab.Targeting.Mode, "self", StringComparison.OrdinalIgnoreCase))
            return false;

        var apps = ab.Resolution.ApplyStatuses;
        if (apps is null || apps.Count == 0) return false;

        foreach (var ap in apps)
        {
            var existing = self.GetStatus(ap.StatusId);
            if (existing is null) continue;
            if (existing.RemainingTurns > 1)
                return true;
        }
        return false;
    }

    private static bool IsPointlessFreeStep(BattleState s, ActorInstance self, AbilityTemplateDto ab, List<ActorInstance> enemies, bool threatened)
    {
        if (!string.Equals(ab.Targeting.Mode, "self", StringComparison.OrdinalIgnoreCase))
            return false;
        if (!IsFreeStep(ab)) return false;
        if (!threatened) return true;

        foreach (var abId in self.Template.Abilities)
        {
            if (IsPureDefense(abId)) continue;
            if (!s.Content.AbilityById.TryGetValue(abId, out var off)) continue;
            if (self.Mp < off.MpCost) continue;

            bool isOffensive =
                string.Equals(off.Resolution.Type, "attack", StringComparison.OrdinalIgnoreCase) ||
                (off.Resolution.ApplyStatuses is { Count: > 0 } && !string.Equals(off.Resolution.Type, "heal", StringComparison.OrdinalIgnoreCase));

            if (!isOffensive) continue;

            var (ok, tgt) = FindTargetForAbility(s, self, off, enemies);
            if (ok && tgt is not null && tgt != self)
                return true;
        }

        return false;
    }
}
