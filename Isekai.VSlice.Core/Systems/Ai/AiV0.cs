using Isekai.VSlice.Core.Content.Dto;
using Isekai.VSlice.Core.Runtime;

namespace Isekai.VSlice.Core.Systems.Ai;

public static class AiV0
{
    // ═══════════════════════════════════════════════════════════════
    //  DECISION RECORD (unchanged)
    // ═══════════════════════════════════════════════════════════════

    public sealed record Decision(string? AbilityId, ActorInstance? Target, int? MoveX, int? MoveY)
    {
        public bool HasMove => MoveX.HasValue && MoveY.HasValue;
        public bool HasAbility => AbilityId is not null && Target is not null;

        public static Decision Wait() => new(null, null, null, null);
        public static Decision MoveOnly(int x, int y) => new(null, null, x, y);
        public static Decision ActOnly(string abilityId, ActorInstance target) => new(abilityId, target, null, null);
        public static Decision MoveAndAct(int x, int y, string abilityId, ActorInstance target) => new(abilityId, target, x, y);
    }

    // ═══════════════════════════════════════════════════════════════
    //  SCORED CANDIDATE (new)
    // ═══════════════════════════════════════════════════════════════

    private readonly record struct ActionCandidate(
        Decision Decision,
        double Score,
        int BudgetCost);

    // ═══════════════════════════════════════════════════════════════
    //  TUNING CONSTANTS
    // ═══════════════════════════════════════════════════════════════

    // Movement
    public const int MoveRange = 4;

    // Threat geometry
    private const int ThreatCloseRange  = 2;   // 1–2 tiles = close
    private const int ThreatFarRange    = 4;   // 3–4 tiles = medium
    private const double ThreatWeightClose  = 3.0;
    private const double ThreatWeightMedium = 1.0;

    // Action scoring
    private const double DamageScorePerHp          = 1.0;
    private const double KillBonusMultiplier        = 2.5;   // multiplicative on capped damage
    private const double StatusDebuffScore          = 15.0;  // flat bonus, debuff on enemy
    private const double StatusBuffScore            = 10.0;  // flat bonus, buff on self
    private const double GuardScoreBase             = 8.0;
    private const double GuardThreatWeight          = 2.0;   // guard score += threat × this
    private const double PositionImprovementPerTile = 2.5;   // move-only: per tile closer

    // Patience (wait / retreat)
    private const double WaitPreferenceThreshold = 1.3;   // projected must beat current by 30%
    private const double RetreatThreatFloor      = 6.0;   // minimum threat to consider retreat
    private const double LowValueActionFloor     = 5.0;   // below this, even weak patience pays

    // Heal triage (unchanged)
    private const double HealThresholdSupport = 0.75;
    private const double HealThresholdGeneral = 0.50;

    // Cleansable negatives (unchanged — forward-looking IDs are harmless)
    private static readonly HashSet<string> CleansableNegativeStatusIds =
        new(StringComparer.Ordinal)
        {
            "st_slowed", "st_weakened", "st_sundered",
            "st_blinded", "st_silenced", "st_cursed",
        };

    // ═══════════════════════════════════════════════════════════════
    //  MAIN ENTRY POINT
    // ═══════════════════════════════════════════════════════════════

    public static Decision ChooseAction(
        BattleState s, ActorInstance self,
        int remainingBudget, bool usedFreeAction)
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

        // ─────────────────────────────────────────────────────
        // 1) HEAL TRIAGE — immediate, bypasses scoring
        // ─────────────────────────────────────────────────────
        var healDecision = TryHealTriage(s, self, allies, enemies, remainingBudget, usedFreeAction);
        if (healDecision is not null) return healDecision;

        // ─────────────────────────────────────────────────────
        // 2) GENERATE & SCORE CANDIDATES
        // ─────────────────────────────────────────────────────
        var candidates = GenerateScoredCandidates(
            s, self, enemies, remainingBudget, usedFreeAction,
            self.X, self.Y, threatened);

        ActionCandidate? bestNow = candidates.Count > 0
            ? candidates.OrderByDescending(c => c.Score).First()
            : null;

        // ─────────────────────────────────────────────────────
        // 3) PATIENCE EVALUATION (wait / retreat)
        // ─────────────────────────────────────────────────────
        var patienceDecision = EvaluatePatience(
            s, self, enemies, remainingBudget, bestNow);
        if (patienceDecision is not null) return patienceDecision;

        // ─────────────────────────────────────────────────────
        // 4) EXECUTE BEST CANDIDATE
        // ─────────────────────────────────────────────────────
        if (bestNow is not null)
        {
            // Track skirmish use
            if (bestNow.Value.Decision.HasAbility &&
                s.Content.AbilityById.TryGetValue(bestNow.Value.Decision.AbilityId!, out var chosen) &&
                IsFreeStep(chosen) && threatened)
            {
                self.UsedSkirmishThisContact = true;
            }

            return bestNow.Value.Decision;
        }

        // ─────────────────────────────────────────────────────
        // 5) FALLBACK: WAIT
        // ─────────────────────────────────────────────────────
        return Decision.Wait();
    }

    // ═══════════════════════════════════════════════════════════════
    //  HEAL TRIAGE (budget-aware)
    // ═══════════════════════════════════════════════════════════════

    private static Decision? TryHealTriage(
        BattleState s, ActorInstance self,
        List<ActorInstance> allies, List<ActorInstance> enemies,
        int remainingBudget, bool usedFreeAction)
    {
        // Collect affordable heal abilities
        var heals = new List<AbilityTemplateDto>();
        foreach (var abId in self.Template.Abilities)
        {
            if (!s.Content.AbilityById.TryGetValue(abId, out var ab)) continue;
            if (!string.Equals(ab.Resolution.Type, "heal", StringComparison.OrdinalIgnoreCase)) continue;
            if (!CanAfford(self, ab, remainingBudget, usedFreeAction)) continue;
            heals.Add(ab);
        }

        if (heals.Count == 0) return null;

        bool isSupport = self.Template.RoleTags?.Contains("support") ?? false;
        double threshold = isSupport ? HealThresholdSupport : HealThresholdGeneral;

        var hurtAllies = allies
            .Where(a => a.IsAlive && (double)a.Hp / a.Template.BaseStats.MaxHp <= threshold)
            .OrderBy(a => (double)a.Hp / a.Template.BaseStats.MaxHp)
            .ThenBy(a => a.InstanceId, StringComparer.Ordinal)
            .ToList();

        if (hurtAllies.Count == 0) return null;

        // Try healing from current position
        foreach (var heal in heals)
        {
            if (heal.CtCost > remainingBudget) continue;
            foreach (var ally in hurtAllies)
            {
                if (IsHealAtFullHp(heal, ally)) continue;
                if (CanTargetWith(s, self.X, self.Y, heal, ally))
                    return Decision.ActOnly(heal.AbilityId, ally);
            }
        }

        // Try moving toward most injured ally, then healing
        var primaryTarget = hurtAllies[0];
        var (mx, my, moveSteps) = PathToward(s, self, self.X, self.Y, primaryTarget.X, primaryTarget.Y, MoveRange);
        int moveCost = moveSteps * TurnEngine.MoveCostPerTile;

        if (moveSteps > 0 && moveCost <= remainingBudget)
        {
            int budgetAfterMove = remainingBudget - moveCost;
            foreach (var heal in heals)
            {
                if (heal.CtCost > budgetAfterMove) continue;
                foreach (var ally in hurtAllies)
                {
                    if (IsHealAtFullHp(heal, ally)) continue;
                    if (CanTargetWith(s, mx, my, heal, ally))
                        return Decision.MoveAndAct(mx, my, heal.AbilityId, ally);
                }
            }
        }

        return null;
    }

    // ═══════════════════════════════════════════════════════════════
    //  CANDIDATE GENERATION & SCORING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Generates all feasible action candidates from the given origin position
    /// and scores them. Used for both actual-turn evaluation and projections.
    /// </summary>
    private static List<ActionCandidate> GenerateScoredCandidates(
        BattleState s, ActorInstance self, List<ActorInstance> enemies,
        int budget, bool usedFreeAction,
        int originX, int originY, bool threatened)
    {
        var candidates = new List<ActionCandidate>();

        bool isEnemy = self.Faction.Equals("enemy", StringComparison.OrdinalIgnoreCase);

        // ── Act-only from origin position ──────────────────────
        foreach (var abId in self.Template.Abilities)
        {
            if (!s.Content.AbilityById.TryGetValue(abId, out var ab)) continue;
            if (!PassesGates(self, ab, budget, usedFreeAction, threatened)) continue;

            var targets = FindScoredTargets(s, self, ab, enemies, originX, originY);
            foreach (var (target, score) in targets)
            {
                int abCost = TurnEngine.IsFreeActionAbility(ab) ? 0 : ab.CtCost;
                candidates.Add(new ActionCandidate(
                    Decision.ActOnly(ab.AbilityId, target),
                    score,
                    abCost));
            }
        }

        // ── Move + Act toward nearest enemy ────────────────────
        var nearest = enemies
            .OrderBy(e => s.Manhattan(originX, originY, e.X, e.Y))
            .ThenBy(e => e.InstanceId, StringComparer.Ordinal)
            .FirstOrDefault();

        if (nearest is not null)
        {
            var (mx, my, moveSteps) = PathToward(s, self, originX, originY, nearest.X, nearest.Y, MoveRange);
            int moveCost = moveSteps * TurnEngine.MoveCostPerTile;

            if (moveSteps > 0 && moveCost <= budget)
            {
                int budgetAfterMove = budget - moveCost;

                foreach (var abId in self.Template.Abilities)
                {
                    if (!s.Content.AbilityById.TryGetValue(abId, out var ab)) continue;
                    if (IsFreeStep(ab)) continue; // don't free-step after a full move
                    if (!PassesGates(self, ab, budgetAfterMove, usedFreeAction, threatened)) continue;

                    var targets = FindScoredTargets(s, self, ab, enemies, mx, my);
                    foreach (var (target, score) in targets)
                    {
                        int abCost = TurnEngine.IsFreeActionAbility(ab) ? 0 : ab.CtCost;
                        candidates.Add(new ActionCandidate(
                            Decision.MoveAndAct(mx, my, ab.AbilityId, target),
                            score,
                            moveCost + abCost));
                    }
                }

                // ── Move-only (advance toward enemy) ───────────
                int currentDist = s.Manhattan(originX, originY, nearest.X, nearest.Y);
                int newDist = s.Manhattan(mx, my, nearest.X, nearest.Y);
                int closerBy = currentDist - newDist;

                if (closerBy > 0)
                {
                    candidates.Add(new ActionCandidate(
                        Decision.MoveOnly(mx, my),
                        closerBy * PositionImprovementPerTile,
                        moveCost));
                }
            }
        }

        // ── Guard / Brace ──────────────────────────────────────
        // Enemies never guard when they have offense available from origin.
        bool suppressGuard = isEnemy && candidates.Any(c =>
            c.Decision.HasAbility &&
            s.Content.AbilityById.TryGetValue(c.Decision.AbilityId!, out var ca) &&
            string.Equals(ca.Resolution.Type, "attack", StringComparison.OrdinalIgnoreCase));

        if (threatened && !suppressGuard)
        {
            double threat = ComputeThreatAtPosition(s, originX, originY, enemies);

            foreach (var abId in self.Template.Abilities)
            {
                if (!IsPureDefense(abId)) continue;
                if (!s.Content.AbilityById.TryGetValue(abId, out var ab)) continue;
                if (!CanAfford(self, ab, budget, usedFreeAction)) continue;

                // Guard targets self
                double guardScore = GuardScoreBase + threat * GuardThreatWeight;
                int abCost = ab.CtCost;

                candidates.Add(new ActionCandidate(
                    Decision.ActOnly(ab.AbilityId, self),
                    guardScore,
                    abCost));
            }
        }

        return candidates;
    }

    /// <summary>
    /// Finds all valid targets for an ability from a given origin and scores each.
    /// Returns (target, score) pairs.
    /// </summary>
    private static List<(ActorInstance target, double score)> FindScoredTargets(
        BattleState s, ActorInstance self, AbilityTemplateDto ab,
        List<ActorInstance> enemies, int originX, int originY)
    {
        var results = new List<(ActorInstance, double)>();

        if (string.Equals(ab.Targeting.Mode, "self", StringComparison.OrdinalIgnoreCase))
        {
            if (IsHealAtFullHp(ab, self)) return results;

            double score = ScoreAbilityOnTarget(self, self, ab, enemies, s);
            if (score > 0)
                results.Add((self, score));
            return results;
        }

        bool wantsAlly = ab.Targeting.Mode.Contains("ally", StringComparison.OrdinalIgnoreCase);
        var pool = wantsAlly
            ? s.AliveActors.Where(a => a.IsAlive && a.Faction == self.Faction).ToList()
            : enemies;

        foreach (var c in pool
                     .OrderBy(c => s.Manhattan(originX, originY, c.X, c.Y))
                     .ThenBy(c => c.InstanceId, StringComparer.Ordinal))
        {
            if (!CanTargetWith(s, originX, originY, ab, c)) continue;
            if (wantsAlly && IsHealAtFullHp(ab, c)) continue;

            double score = ScoreAbilityOnTarget(self, c, ab, enemies, s);
            if (score > 0)
                results.Add((c, score));
        }

        return results;
    }

    // ═══════════════════════════════════════════════════════════════
    //  SCORING
    // ═══════════════════════════════════════════════════════════════

    private static double ScoreAbilityOnTarget(
        ActorInstance self, ActorInstance target, AbilityTemplateDto ab,
        List<ActorInstance> enemies, BattleState s)
    {
        double score = 0;

        // ── Attack scoring ─────────────────────────────────────
        if (string.Equals(ab.Resolution.Type, "attack", StringComparison.OrdinalIgnoreCase))
        {
            double expectedDmg = EstimateExpectedDamage(self, target, ab);
            double cappedDmg = Math.Min(expectedDmg, target.Hp);
            score += cappedDmg * DamageScorePerHp;

            // Kill bonus: significantly reward finishing off enemies
            if (expectedDmg >= target.Hp)
                score *= KillBonusMultiplier;
        }

        // ── Status scoring ─────────────────────────────────────
        if (ab.Resolution.ApplyStatuses is { Count: > 0 })
        {
            bool isEnemyTarget = !target.Faction.Equals(self.Faction, StringComparison.OrdinalIgnoreCase);

            if (isEnemyTarget)
            {
                // Debuffs are more valuable on high-HP (i.e., not-about-to-die) targets
                double hpRatio = (double)target.Hp / target.Template.BaseStats.MaxHp;
                score += StatusDebuffScore * hpRatio;
            }
            else
            {
                // Self-buffs (Focus, etc.)
                score += StatusBuffScore;
            }
        }

        // ── Lifesteal bonus ────────────────────────────────────
        if (ab.Resolution.Lifesteal is not null && score > 0)
        {
            double selfHpRatio = (double)self.Hp / self.Template.BaseStats.MaxHp;
            if (selfHpRatio < 0.75)
                score *= 1.15; // small bonus when hurt: lifesteal is more valuable
        }

        return score;
    }

    /// <summary>
    /// Deterministic expected damage (no RNG). Mirrors Resolver.ComputeDamage
    /// then multiplies by hit chance and crit expected value.
    /// </summary>
    private static double EstimateExpectedDamage(
        ActorInstance attacker, ActorInstance target, AbilityTemplateDto ab)
    {
        if (ab.Resolution.Damage is null) return 0;

        double rawDmg = 0;

        if (ab.Resolution.Damage.Physical is not null)
        {
            var p = ab.Resolution.Damage.Physical;
            int stat = ReadStat(attacker, p.Stat);
            double phys = Math.Max(0, p.Base + stat * p.Scale - target.Def);
            rawDmg += phys;
        }

        if (ab.Resolution.Damage.Elemental is not null)
        {
            var e = ab.Resolution.Damage.Elemental;
            int stat = ReadStat(attacker, e.Stat);
            double elem = Math.Max(0, e.Base + stat * e.Scale);
            elem *= attacker.ProductStatusEffect("elemental_damage_mult");
            elem = Math.Max(0, elem - target.Def / 2.0);
            rawDmg += elem;
        }

        // Incoming damage mult (Guarding 0.75, Exposed 1.2, etc.)
        rawDmg *= target.ProductStatusEffect("incoming_damage_mult");
        rawDmg = Math.Max(1, rawDmg); // v-slice minimum 1 on hit

        // Hit chance
        double hitChance = Clamp(
            0.75
            + attacker.AccuracyMod - target.EvasionMod
            + attacker.SumStatusEffect("accuracy_add")
            + target.SumStatusEffect("attacker_hit_bonus_vs_target"),
            0.05, 0.95);

        // Crit chance
        double critChance = Clamp(
            0.05 + target.SumStatusEffect("attacker_crit_bonus_vs_target"),
            0.0, 0.50);

        // Expected value: damage × P(hit) × (1 + P(crit) × 0.5)
        return rawDmg * hitChance * (1.0 + critChance * 0.5);
    }

    // ═══════════════════════════════════════════════════════════════
    //  THREAT ASSESSMENT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Computes a threat score for standing at (px, py).
    /// Higher = more dangerous. Factors in enemy proximity and ATK stat.
    /// </summary>
    private static double ComputeThreatAtPosition(
        BattleState s, int px, int py, List<ActorInstance> enemies)
    {
        double threat = 0;
        foreach (var e in enemies)
        {
            if (!e.IsAlive) continue;
            int dist = s.Manhattan(px, py, e.X, e.Y);

            if (dist <= ThreatCloseRange)
                threat += e.Atk * ThreatWeightClose / 10.0;
            else if (dist <= ThreatFarRange)
                threat += e.Atk * ThreatWeightMedium / 10.0;
            // Beyond ThreatFarRange: no contribution
        }
        return threat;
    }

    // ═══════════════════════════════════════════════════════════════
    //  PATIENCE EVALUATION (WAIT + RETREAT)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// One-ply lookahead: compares the best immediate action against
    /// projected next-turn value from waiting or retreating.
    /// Returns a Wait or MoveOnly(retreat) decision if patience pays.
    /// Returns null to let the caller execute the best immediate action.
    /// </summary>
    private static Decision? EvaluatePatience(
        BattleState s, ActorInstance self, List<ActorInstance> enemies,
        int remainingBudget, ActionCandidate? bestNow)
    {
        double bestNowScore = bestNow?.Score ?? 0;

        // If current action is very strong, don't even consider waiting
        // (avoids unnecessary computation and prevents dithering)
        if (bestNowScore > 30.0)
            return null;

        bool threatened = IsThreatened(s, self, enemies);

        // ── WAIT at current position ───────────────────────────
        int projBudget = ProjectNextTurnBudget(remainingBudget);

        // Only evaluate wait if projected budget is actually higher
        if (projBudget > remainingBudget)
        {
            var projected = GenerateScoredCandidates(
                s, self, enemies, projBudget, usedFreeAction: false,
                self.X, self.Y, threatened);

            double waitScore = projected.Count > 0
                ? projected.Max(c => c.Score)
                : 0;

            // Patience pays if projected is meaningfully better
            double threshold = bestNowScore < LowValueActionFloor
                ? 1.0   // if current best is junk, any improvement justifies waiting
                : WaitPreferenceThreshold;

            if (waitScore > bestNowScore * threshold)
                return Decision.Wait();
        }

        // ── RETREAT to a safer tile ────────────────────────────
        double currentThreat = ComputeThreatAtPosition(s, self.X, self.Y, enemies);

        if (currentThreat >= RetreatThreatFloor && bestNowScore < LowValueActionFloor * 3)
        {
            var retreat = FindRetreatTile(s, self, enemies, remainingBudget);
            if (retreat is not null)
            {
                var (rx, ry, rSteps) = retreat.Value;
                int retreatCost = rSteps * TurnEngine.MoveCostPerTile;
                int retreatRemaining = remainingBudget - retreatCost;
                int retreatProjBudget = ProjectNextTurnBudget(retreatRemaining);

                bool retreatThreatened = IsThreatened(s, rx, ry, enemies);
                var retreatProj = GenerateScoredCandidates(
                    s, self, enemies, retreatProjBudget, usedFreeAction: false,
                    rx, ry, retreatThreatened);

                double retreatScore = retreatProj.Count > 0
                    ? retreatProj.Max(c => c.Score)
                    : 0;

                // Retreat if projected score from safe tile beats current
                if (retreatScore > bestNowScore * WaitPreferenceThreshold)
                    return Decision.MoveOnly(rx, ry);
            }
        }

        return null; // patience doesn't pay — act now
    }

    /// <summary>
    /// Projects the budget for next turn given how much would be banked.
    /// </summary>
    private static int ProjectNextTurnBudget(int remainingBudget)
    {
        int banked = Math.Clamp(remainingBudget, 0, TurnEngine.MaxBank);
        return TurnEngine.ComputeBudgetFromBank(banked);
    }

    /// <summary>
    /// Finds the reachable tile within MoveRange that minimizes threat score.
    /// Returns null if no tile is meaningfully safer than current position.
    /// </summary>
    private static (int x, int y, int steps)? FindRetreatTile(
        BattleState s, ActorInstance self, List<ActorInstance> enemies,
        int remainingBudget)
    {
        double currentThreat = ComputeThreatAtPosition(s, self.X, self.Y, enemies);
        var reachable = BfsReachable(s, self, MoveRange);

        (int x, int y, int steps)? best = null;
        double bestThreat = currentThreat;

        int maxAffordableSteps = remainingBudget / Math.Max(1, TurnEngine.MoveCostPerTile);

        foreach (var (tile, steps) in reachable)
        {
            if (steps > maxAffordableSteps) continue;

            double threat = ComputeThreatAtPosition(s, tile.x, tile.y, enemies);
            if (threat < bestThreat)
            {
                bestThreat = threat;
                best = (tile.x, tile.y, steps);
            }
        }

        // Only retreat if meaningfully safer (at least 30% reduction)
        if (best is not null && bestThreat < currentThreat * 0.7)
            return best;

        return null;
    }

    // ═══════════════════════════════════════════════════════════════
    //  TARGETING HELPERS
    // ═══════════════════════════════════════════════════════════════

    private static bool CanTargetWith(
        BattleState s, int originX, int originY, AbilityTemplateDto ab, ActorInstance target)
    {
        if (!target.IsAlive) return false;
        int dist = s.Manhattan(originX, originY, target.X, target.Y);
        if (dist > ab.Targeting.Range) return false;
        if (ab.Targeting.RequiresLos && !Systems.Los.HasLineOfSight(s, originX, originY, target.X, target.Y))
            return false;
        return true;
    }

    /// <summary>
    /// Checks affordability: MP cost + CT budget + free-action limit.
    /// </summary>
    private static bool CanAfford(
        ActorInstance self, AbilityTemplateDto ab, int remainingBudget, bool usedFreeAction)
    {
        if (self.Mp < ab.MpCost) return false;
        if (TurnEngine.IsFreeActionAbility(ab))
            return !usedFreeAction;
        return ab.CtCost >= 1 && ab.CtCost <= remainingBudget;
    }

    // ═══════════════════════════════════════════════════════════════
    //  PATHFINDING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// BFS toward goal from an arbitrary start position.
    /// Returns the reachable tile closest to goal and the number of BFS steps to reach it.
    /// </summary>
    private static (int x, int y, int steps) PathToward(
        BattleState s, ActorInstance self,
        int startX, int startY,
        int goalX, int goalY, int maxSteps)
    {
        var start = (startX, startY);
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
        int bestSteps = 0;

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
                bestSteps = steps;
            }
        }

        return (best.Item1, best.Item2, bestSteps);
    }

    /// <summary>
    /// BFS from actor's current position. Returns all reachable tiles and their step count.
    /// Used by FindRetreatTile for threat evaluation at each reachable position.
    /// </summary>
    private static List<((int x, int y) tile, int steps)> BfsReachable(
        BattleState s, ActorInstance self, int maxSteps)
    {
        var start = (self.X, self.Y);
        var q = new Queue<(int x, int y)>();
        var dist = new Dictionary<(int x, int y), int>();
        var results = new List<((int, int), int)>();

        q.Enqueue(start);
        dist[start] = 0;

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            int d = dist[cur];

            if (d > 0) // exclude start position
                results.Add((cur, d));

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

        return results;
    }

    // ═══════════════════════════════════════════════════════════════
    //  GATE CHECKS (which abilities to even consider)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns true if the ability passes all pre-scoring gates.
    /// These are correctness filters, not quality judgments —
    /// scoring handles "is this GOOD?", gates handle "is this LEGAL?"
    /// </summary>
    private static bool PassesGates(
        ActorInstance self, AbilityTemplateDto ab,
        int remainingBudget, bool usedFreeAction, bool threatened)
    {
        // Pure defense handled separately in candidate generation
        if (IsPureDefense(ab.AbilityId)) return false;

        // Affordability
        if (!CanAfford(self, ab, remainingBudget, usedFreeAction)) return false;

        // Heals handled by triage
        if (string.Equals(ab.Resolution.Type, "heal", StringComparison.OrdinalIgnoreCase)) return false;

        // Cleanse: only when we have something cleansable
        if (string.Equals(ab.AbilityId, "ab_cleanse", StringComparison.Ordinal) && !HasCleansableNegative(self))
            return false;

        // Free-step: only when threatened and not already used this contact
        if (IsFreeStep(ab))
        {
            if (!threatened) return false;
            if (self.UsedSkirmishThisContact) return false;
        }

        // Redundant self-buff: don't refresh with >1 turn remaining
        if (string.Equals(ab.Targeting.Mode, "self", StringComparison.OrdinalIgnoreCase)
            && IsRedundantSelfBuff(ab, self))
            return false;

        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    //  CLASSIFICATION HELPERS
    // ═══════════════════════════════════════════════════════════════

    private static bool IsThreatened(BattleState s, ActorInstance self, List<ActorInstance> enemies)
        => IsThreatened(s, self.X, self.Y, enemies);

    private static bool IsThreatened(BattleState s, int x, int y, List<ActorInstance> enemies)
        => enemies.Any(e => e.IsAlive && s.Manhattan(x, y, e.X, e.Y) <= ThreatCloseRange + 1);

    private static bool IsPureDefense(string abilityId)
        => abilityId is "ab_guard" or "ab_brace";

    private static bool IsFreeStep(AbilityTemplateDto ab)
        => ab.Resolution.Utility is not null
           && string.Equals(ab.Resolution.Utility.Kind, "free_step", StringComparison.OrdinalIgnoreCase);

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

    private static int ReadStat(ActorInstance a, string? stat) => stat switch
    {
        "atk" => a.Atk,
        "def" => a.Def,
        "int" => a.Int,
        "wis" => a.Wis,
        "none" or null => 0,
        _ => 0
    };

    private static double Clamp(double v, double lo, double hi) => (v < lo) ? lo : (v > hi) ? hi : v;
}
