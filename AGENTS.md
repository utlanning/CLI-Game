Task: Fix failing UnitTest1 in a principled, deterministic way (no AI/RNG coupling).

Context:
- UnitTest1.cs currently fails at Assert.Equal(86, enemy.Hp) inside ValidationAndTerminationInvariants_AreEnforced().
- The test uses BattleRunner.RunAuto(state, maxRounds: 1) and assumes a specific attack landed once.
- This is brittle because RunAuto is AI-driven and Resolver.DoAttack is RNG-driven (hit/crit).

Required changes:
1) Edit Isekai.VSlice.Tests/UnitTest1.cs
   - In ValidationAndTerminationInvariants_AreEnforced(), remove the integration section:
     - Remove BattleRunner.RunAuto(...) and the enemy.Hp == 86 assertion.
   - Keep the deterministic validation assertions:
     - invalidPrimary produces message containing "ab_bad_primary" and "ct_cost >= 1"
     - invalidFree produces message containing "ab_bad_free" and "ct_cost == 0"
     - TurnEngine.MoveCostPerTile >= 1

2) Add a new deterministic test for "cantrip/free action slot-locked to 1 per turn"
   - Do NOT use BattleRunner.RunAuto or AiV0.
   - Do NOT use attack resolution (hit/crit RNG).
   - Instead, create a 0-CT “free action” ability that produces a deterministic observable effect:
     - Prefer Resolution.Type = "utility" with ApplyStatuses applying a test status, OR another deterministic utility effect.
   - Create a minimal Turn execution seam that enforces the free-action-per-turn rule, then test it:
     - Attempt to use the free action twice in the same turn.
     - Assert only one application occurred (e.g., status count == 1, or log contains exactly one “status:” line for that status).

Implementation guidance (choose minimal-impact path):
A) If the new turn/budget executor already tracks per-turn flags like “usedFreeAction/cantripUsed”, call it directly from the unit test.
   - Use ripgrep to find how free actions are identified (utility kind “free_step” or name contains “cantrip” or similar) and how the per-turn use is recorded.
   - Write the test around that.

B) If no public seam exists, create a small one in TurnEngine (preferred) or BattleRunner:
   - e.g., TurnEngine.TryUseAbilityInTurn(actor, ability, target, ref remainingBudget, ref usedFreeAction)
   - Requirements:
     - If ability is free/cantrip (ctCost==0 & identified as free), allow only once per turn.
     - Using a blocked second free action must have no effect and must not change budget.
     - Budget must still monotonically decrease for non-free actions (ctCost>=1).
   - Keep it minimal; do not refactor major game logic for this.

3) Optional: If you still want a “damage math = 14” test, add it separately as a unit test that bypasses hit RNG.
   - Either expose a deterministic ComputeDamage hook (public/internal) OR test a pure damage method if it exists.
   - Do not assert HP changes via RunAuto.

Acceptance criteria:
- `dotnet test` passes reliably (no flaky AI/RNG dependence).
- UnitTest1 has:
  - deterministic CT normalization/wait banking tests (unchanged)
  - deterministic content validation tests (unchanged)
  - deterministic free-action slot-lock test (new), without calling RunAuto.

Commands:
- Run: `dotnet test`
