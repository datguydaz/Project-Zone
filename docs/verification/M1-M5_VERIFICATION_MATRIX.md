# M1–M5 Verification Matrix

**Purpose:** Preserve reported milestone completion while separating **missing** Play Mode re-checks and **performance** evidence.  
**Rule:** Code existing in the repo is **not** sufficient to invent completion. This matrix records what was actually confirmed vs what remains unproven.

Legend:

| Symbol | Meaning |
| --- | --- |
| **RC** | Reported complete (developer Play Mode / process confirmation recorded in changelog or issue close) |
| **PM?** | Play Mode confirmation incomplete, outdated, or needs re-check |
| **PERF−** | No standalone FPS/frame-time baseline for this scope |
| **DEFER** | Explicitly deferred polish / later milestone (not a silent failure) |

## Milestone status (do not reopen)

| Milestone | GitHub | Reported status | Honesty notes |
| --- | --- | --- | --- |
| M1 Foundation | Closed | **RC** | Developer-confirmed foundation. **PERF−** for whole project. |
| M2 Emerald | Closed | **RC** | Adapters/detection/death/package checks Play Mode confirmed. Melee was deferred at close; Female Zombie greybox added later → **PM?** for melee checklist (#11 spirit). **PERF−**. |
| M3 Outskirts | Closed | **RC** | Greybox layout/NPCs/atmosphere systems Play Mode confirmed. Art/fog/real underground load = **DEFER** (M6–M7). **PERF−**. |
| M4 Inventory | Closed (issues #21–#26 closed) | **RC** | Play Mode confirmed stash/loot/wheel/loadout. **PERF−**. API may show stale milestone open_issue counters; issue states are closed. |
| M5 Death & Contracts | Closed | **RC** | Play Mode confirmed hybrid death/caches/wallet/contracts/loop. **PERF−**. |

## Cross-cutting gaps (not grounds to un-complete M1–M5)

| Gap | Severity | Action |
| --- | --- | --- |
| No standalone FPS baseline / 1% lows | High | M5.5 procedure ready; developer run required (`docs/performance/BASELINE.md`) |
| Reference PC fields empty | High | Fill in BASELINE.md |
| Melee combat validator checkbox after zombies | Medium | Optional Play Mode re-check on Outskirts; do not reopen M2 unless regression found |
| Additive multi-region persistence | Medium | Untested; M5.5 risk → M6 gate |
| Emerald Animation Profile for Female Zombie | Medium | Greybox OK; Evaluation |
| Fantasy/gothic packs on disk | Medium | Excluded in asset registry; do not productionize |
| Git LFS incomplete for some binaries | High | Remediate before more large imports |
| PZ save `schemaVersion` missing | Medium | Before M10 economy expansion |

## How agents must treat M1–M5

1. **Preserve** closed milestones and closed issues unless the developer reports a regression.
2. **Never** mark historical work complete solely because scripts/scenes exist.
3. Record new evidence in changelog + this matrix (or M5.5+ issues).
4. Performance claims require measured standalone (or explicitly labeled Editor-only diagnostics).
