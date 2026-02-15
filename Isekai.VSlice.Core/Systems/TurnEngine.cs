using Isekai.VSlice.Core.Runtime;

namespace Isekai.VSlice.Core.Systems;

public static class TurnEngine
{
    public const double CtThreshold = 100.0;

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
            a.Ct += a.Speed * minDelta;

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
        actor.Ct -= ctCost;
        if (actor.Ct < 0) actor.Ct = 0;
    }
}
