# CLI-Game — Agent Handoff (CT/Turn System Stabilization)

This document captures the current design decisions and the required code changes to stabilize and scale the combat core, focusing on CT, Wait, and bounded turn execution.

## Scope & Goals
**Primary goals**
- Correctness: deterministic, terminating turns; no degenerate loops.
- Scalability: predictable runtime behavior; avoid O(n) map lookups in hot paths (follow-up item).
- Testability: encode invariants with unit tests.

**Non-goals (explicitly deferred)**
- Opacity / fog / vision/LoS overhaul.
- Pathfinding correctness beyond “basic movement respects obstacles + occupancy”.
- UI polish.

---

## Glossary
- **CT**: initiative meter that accrues over time based on speed.
- **Eligible**: CT >= 100 triggers the actor to be selectable.
- **Spend Budget**: per-turn budget used for movement and abilities.
- **Bank**: bounded carryover (0..60) used to increase per-turn spend budget on next eligible turn.
- **Cap**: global hard cap of 160 on CT meter and per-turn spend.

---

## Finalized Rules

### R1. Global hard caps
- CT meter is clamped to: `0..160`
- Per-turn Spend Budget is clamped to: `100..160`

### R2. Eligibility
- An actor becomes eligible when `CT >= 100`.

### R3. Overflow normalization at turn start (prevents double-counting)
When an eligible actor is selected to act:
1. Compute overflow: `overflow = clamp(CT - 100, 0, 60)`
2. Set `CT = 100` (normalize at threshold)
3. Update bank: `Bank = clamp(Bank + overflow, 0, 60)`
4. Compute spend budget: `Budget = 100 + Bank` (=> 100..160)

Rationale: If the actor overshoots above 100 due to other units acting, that “extra time” is converted into bank once, and CT is normalized to avoid treating CT itself as a second spend pool.

### R4. In-turn execution: move → primary → move → primary
- Turns are interleavable: movement and primaries can be alternated until budget is exhausted or the actor ends the turn.
- Budget is a strictly decreasing counter during a turn; any repeatable action must reduce it.

### R5. Action slots
- **Movement**: costs CT per step (must be >= 1). Consumes from Budget.
- **Primaries**: cost CT (must be >= 1). Unlimited count per turn, subject to Budget.
- **Cantrip / Free Action**: may have 0 CT cost, but is limited to **1 use per turn** (slot-lock). Must not grant immediate additional budget/actions.
- **Wait**: special end-turn action defined below.

### R6. Wait behavior (A + H2 under 160 cap)
Wait is intended to be tempo-neutral and a viable way to “bank for next turn” without forcing a full rebuild from 0.

On Wait:
- End turn immediately.
- Set initiative head-start: `CT = 60`.
- Bank based on remaining budget: `Bank = clamp(RemainingBudget, 0, 60)`

Where:
- `RemainingBudget = Budget - SpentThisTurn`
- If RemainingBudget >= 60, Bank becomes 60 (next eligible turn can spend 160).
- If RemainingBudget is small, Bank is small (next eligible budget is 100+Bank).

On normal End Turn (not Wait):
- `CT = 0`
- `Bank = 0`

### R7. Termination invariants (degenerate-loop prevention)
- Any repeatable budget-consuming action must have `CtCost >= 1`.
- Any free action with `CtCost == 0` must be limited to a per-turn use count (currently: 1).
- No intra-turn effect may increase `Budget` (CT refunds, negative costs, or “gain CT now” effects are prohibited in this phase; if introduced later, they must apply at end-of-turn to CT/Bank, not Budget).

---

## Required Code Changes (high level)

### C1. TurnEngine / selection
- Allow CT to accrue up to 160 (cap).
- When selecting the next ready actor, do not clamp them down to 100 immediately. Normalize at *turn start* using R3.

### C2. BattleRunner / turn loop
- Replace single-action execution with an intra-turn loop:
  - Initialize `Budget = 100 + Bank` (after normalization).
  - Track per-turn usage flags: `usedFreeAction = false`.
  - Allow sequences: move (stepwise) / primary / move / primary.
  - Decrement Budget strictly for movement steps and primaries.
  - Enforce cantrip limit: if `usedFreeAction == true`, cantrip option is unavailable.
  - End turn sets CT/Bank based on whether the actor chose Wait or normal End.

### C3. Wait implementation
- Implement Wait exactly per R6.
- Ensure after Wait: CT is 60 (not >=100) so the actor cannot immediately re-trigger readiness.

### C4. Content validation (load-time)
Reject or error loudly if:
- Any primary ability has `CtCost <= 0`
- Movement step cost <= 0
- Free/cantrip actions have CtCost != 0, or exceed per-turn limit
- Any effect specifies “refund CT immediately” (unless explicitly allowed by a future ruleset)

### C5. AI simulation safety (near-term)
- AI must not mutate real actor coordinates to “simulate” positions.
- Use local variables or a planning snapshot for range checks.

---

## Tests to Add (minimum set)

### T1. Turn start normalization
- Given CT=137 and Bank=10, after normalization:
  - CT == 100
  - overflow == 37
  - Bank == min(60, 10+37) == 47
  - Budget == 147

### T2. Wait banking behavior (H2)
- Start: CT=100, Bank=0 => Budget=100
  - Spend 80, RemainingBudget=20
  - Wait => CT=60, Bank=20
  - Next eligible => Budget=120

- Start: CT=100, Bank=0 => do nothing
  - Wait => CT=60, Bank=60
  - Next eligible => Budget=160

### T3. Termination invariants
- Validate loader rejects CtCost <= 0 for primaries.
- Validate move step cost must be >= 1.
- Validate cantrip is limited to 1 per turn.

### T4. Interleaving rule
- With Budget=120, ensure actor can move (cost 20), primary (cost 60), move (cost 20), then is left with 20 and cannot execute a 60-cost primary.

---

## Acceptance Criteria (definition of done)
- Turns always terminate without requiring a gameplay “max actions per turn”.
- Wait produces the intended head-start and bounded banking, under the global 160 cap.
- Per-turn spend budget never exceeds 160; CT meter never exceeds 160.
- No repeated-action degeneracy possible via 0-cost primaries or 0-cost movement (blocked by validation).
- Unit tests cover the CT normalization + Wait banking rules.

---

## Review Guidelines (for agents / code review)
- Treat violations of the invariants in R7 as P0.
- Do not introduce O(n) per-tile lookups in hot loops; prefer O(1) grids (follow-up task).
- Avoid mutating runtime state inside AI evaluation unless explicitly executing a committed action.
