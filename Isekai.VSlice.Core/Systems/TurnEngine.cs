using Isekai.VSlice.Core.Content.Dto;
using Isekai.VSlice.Core.Runtime;

namespace Isekai.VSlice.Core.Systems;

public static class TurnEngine
{
    public const double CtThreshold = 100.0;
    public const double CtCap = 160.0;
    public const int MaxBank = 60;
    public const int BaseTurnBudget = 100;
    public const int MaxTurnBudget = 160;

    // --- NEW: Canonical CT delay costs per PART_2 G.3.3 ---
    public const int MoveCostPerTile = 20;   // +20 per tile moved
    public const int WaitCost         = 50;   // Pass turn

    public static ActorInstance GetNextReady(BattleState s)
    {
        // Advance time until someone reaches threshold
        var alive = s.AliveActors.ToList();
        if (alive.Count == 0) throw new InvalidOperationException("No alive actors.");

        double minDelta = double.PositiveInfinity;

        foreach (var a in alive)
        {
            if (a.Speed <= 0) continue;
            if (a.Ct >= CtThreshold) { minDelta = 0; break; }
            var delta = (CtThreshold - a.Ct) / a.Speed;
            if (delta < minDelta) minDelta = delta;
        }

        if (double.IsPositiveInfinity(minDelta)) minDelta = 0;

        foreach (var a in alive)
            a.Ct = ClampCt(a.Ct + a.Speed * minDelta);

        // Ready set
        var ready = alive.Where(a => a.Ct >= CtThreshold).ToList();
        if (ready.Count == 0)
        {
            // numeric drift fallback: pick max CT
            ready = alive.OrderByDescending(a => a.Ct).Take(1).ToList();
        }

        // Tie-break: highest CT, then speed, then stable InstanceId
        return ready
            .OrderByDescending(a => a.Ct)
            .ThenByDescending(a => a.Speed)
            .ThenBy(a => a.InstanceId, StringComparer.Ordinal)
            .First();
    }

    // --- CHANGED: Now accepts the action's CT cost instead of always subtracting 100 ---
    public static void ConsumeTurn(ActorInstance actor, int ctCost)
    {
        actor.Ct = ClampCt(actor.Ct - ctCost);
    }

    public static int NormalizeReadyActorAndComputeBudget(ActorInstance actor)
    {
        actor.Ct = ClampCt(actor.Ct);

        var overflow = Math.Clamp((int)Math.Floor(actor.Ct - CtThreshold), 0, MaxBank);
        actor.Ct = CtThreshold;
        actor.CtBank = Math.Clamp(actor.CtBank + overflow, 0, MaxBank);
        return ComputeBudgetFromBank(actor.CtBank);
    }

    public static int ComputeBudgetFromBank(int bank)
        => Math.Clamp(BaseTurnBudget + Math.Clamp(bank, 0, MaxBank), BaseTurnBudget, MaxTurnBudget);

    public static void EndTurnAsWait(ActorInstance actor, int remainingBudget)
    {
        actor.Ct = MaxBank;
        actor.CtBank = Math.Clamp(remainingBudget, 0, MaxBank);
    }

    public static void EndTurnNormally(ActorInstance actor)
    {
        actor.Ct = 0;
        actor.CtBank = 0;
    }

    private static double ClampCt(double ct) => Math.Clamp(ct, 0, CtCap);

    public static bool TrySpendAbilityBudget(AbilityTemplateDto ability, ref int remainingBudget, ref bool usedFreeAction)
    {
        if (IsFreeActionAbility(ability))
        {
            if (usedFreeAction)
                return false;

            usedFreeAction = true;
            return true;
        }

        if (ability.CtCost < 1 || ability.CtCost > remainingBudget)
            return false;

        remainingBudget -= ability.CtCost;
        return true;
    }

    public static bool IsFreeActionAbility(AbilityTemplateDto ability)
    {
        if (ability.CtCost != 0)
            return false;

        return string.Equals(ability.Resolution.Utility?.Kind, "free_step", StringComparison.OrdinalIgnoreCase);
    }

}
