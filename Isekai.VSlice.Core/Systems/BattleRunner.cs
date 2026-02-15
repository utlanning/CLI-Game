using Isekai.VSlice.Core.Runtime;
using Isekai.VSlice.Core.Systems.Ai;

namespace Isekai.VSlice.Core.Systems;

public static class BattleRunner
{
    public static void RunAuto(BattleState s, int maxRounds = 200)
    {
        if (TurnEngine.MoveCostPerTile <= 0)
            throw new InvalidOperationException("MoveCostPerTile must be >= 1.");

        s.Log.AddHeader("BATTLE");

        while (!s.IsWin && !s.IsLose && s.RoundCounter < maxRounds)
        {
            var actor = TurnEngine.GetNextReady(s);

            if (!actor.IsAlive)
            {
                TurnEngine.EndTurnNormally(actor);
                continue;
            }

            s.RoundCounter++;
            var budget = TurnEngine.NormalizeReadyActorAndComputeBudget(actor);
            var turnBudget = budget;
            var usedFreeAction = false;
            var waited = false;

            Resolver.StartOfTurn(s, actor);
            if (!actor.IsAlive)
            {
                TurnEngine.EndTurnNormally(actor);
                continue;
            }

            while (budget > 0 && actor.IsAlive)
            {
                // --- CHANGED: Pass budget and free-action state to AI ---
                var decision = AiV0.ChooseAction(s, actor, budget, usedFreeAction);
                var didSomething = false;

                if (decision.HasMove)
                {
                    int tilesMoved = s.Manhattan(actor.X, actor.Y, decision.MoveX!.Value, decision.MoveY!.Value);
                    int moveCost = tilesMoved * TurnEngine.MoveCostPerTile;

                    if (tilesMoved > 0 && moveCost <= budget)
                    {
                        Resolver.ExecuteMove(s, actor, decision.MoveX.Value, decision.MoveY.Value);
                        budget -= moveCost;
                        didSomething = true;
                    }
                }

                if (decision.HasAbility)
                {
                    var ab = s.Content.AbilityById[decision.AbilityId!];

                    if (!TurnEngine.TrySpendAbilityBudget(ab, ref budget, ref usedFreeAction))
                    {
                        if (TurnEngine.IsFreeActionAbility(ab) && usedFreeAction)
                            s.Log.Add($"{actor.ColorTag} cannot use another free action this turn.");
                    }
                    else if (decision.Target is not null)
                    {
                        s.Log.Add($"{actor.ColorTag} acts: {ab.AbilityId} -> {decision.Target.ColorTag}");
                        Resolver.ExecuteAbility(s, actor, ab, decision.Target);
                        didSomething = true;
                    }
                }

                if (!didSomething)
                {
                    s.Log.Add($"{actor.ColorTag} waits.");
                    waited = true;
                    TurnEngine.EndTurnAsWait(actor, budget);
                    break;
                }
            }

            if (!waited)
                TurnEngine.EndTurnNormally(actor);

            Resolver.EndOfTurn(s, actor);
            s.Log.Add($"  \x1b[90m[Budget spent: {turnBudget - budget} / {turnBudget}]\x1b[0m");
        }

        s.Log.AddHeader("RESULT");
        if (s.IsWin) s.Log.Add("\x1b[92;1mWIN\x1b[0m");
        else if (s.IsLose) s.Log.Add("\x1b[91;1mLOSE\x1b[0m");
        else s.Log.Add($"\x1b[93mABORT: reached max rounds {maxRounds}\x1b[0m");
    }
}
