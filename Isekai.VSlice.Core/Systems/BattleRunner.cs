using Isekai.VSlice.Core.Runtime;
using Isekai.VSlice.Core.Systems.Ai;

namespace Isekai.VSlice.Core.Systems;

public static class BattleRunner
{
    public static void RunAuto(BattleState s, int maxRounds = 200)
    {
        s.Log.AddHeader("BATTLE");

        while (!s.IsWin && !s.IsLose && s.RoundCounter < maxRounds)
        {
            var actor = TurnEngine.GetNextReady(s);

            if (!actor.IsAlive)
            {
                TurnEngine.ConsumeTurn(actor, TurnEngine.WaitCost);
                continue;
            }

            s.RoundCounter++;

            Resolver.StartOfTurn(s, actor);

            if (!actor.IsAlive)
            {
                TurnEngine.ConsumeTurn(actor, TurnEngine.WaitCost);
                continue;
            }

            var decision = AiV0.ChooseAction(s, actor);

            // --- Compute CT cost based on what the decision contains ---
            int ctCost = 0;
            bool didSomething = false;

            // Move phase (if decision includes movement)
            if (decision.HasMove)
            {
                int tilesMoved = s.Manhattan(actor.X, actor.Y, decision.MoveX!.Value, decision.MoveY!.Value);
                Resolver.ExecuteMove(s, actor, decision.MoveX.Value, decision.MoveY.Value);
                ctCost += Math.Max(TurnEngine.MoveCostPerTile, tilesMoved * TurnEngine.MoveCostPerTile);
                didSomething = true;
            }

            // Act phase (if decision includes an ability)
            if (decision.HasAbility)
            {
                var ab = s.Content.AbilityById[decision.AbilityId!];
                s.Log.Add($"{actor.ColorTag} acts: {ab.AbilityId} -> {decision.Target!.ColorTag}");
                Resolver.ExecuteAbility(s, actor, ab, decision.Target);
                ctCost += ab.CtCost;
                didSomething = true;
            }

            // Wait (neither move nor act)
            if (!didSomething)
            {
                s.Log.Add($"{actor.ColorTag} waits.");
                ctCost = TurnEngine.WaitCost;
            }

            s.Log.Add($"  \x1b[90m[CT delay: {ctCost}]\x1b[0m");

            Resolver.EndOfTurn(s, actor);
            TurnEngine.ConsumeTurn(actor, ctCost);
        }

        s.Log.AddHeader("RESULT");
        if (s.IsWin) s.Log.Add("\x1b[92;1mWIN\x1b[0m");
        else if (s.IsLose) s.Log.Add("\x1b[91;1mLOSE\x1b[0m");
        else s.Log.Add($"\x1b[93mABORT: reached max rounds {maxRounds}\x1b[0m");
    }
}
