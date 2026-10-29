# AI Architecture (M5.5 assessment)

## Frameworks

- **Emerald AI 2026** — primary NPC/hostile framework.
- **HQ FPS** — player damage via `HqPlayerDamageBridge` / Emerald player target adapters.
- **PZ greybox** — `PzZombieHostileBrain` + Playables anim driver for Female Zombie until Emerald Animation Profile exists.

## Current state

- Outskirts spawns Female Zombie melee hostiles at Checkpoint/Industrial.
- Optional Emerald ranged spawn flag exists but is off by default.
- Combat validators track player↔AI damage including PZ zombies.

## Multi-region readiness

| Topic | Status | Risk |
| --- | --- | --- |
| NavMesh bake on Outskirts | Present (`NavMeshSurface`) | Additive scenes need per-region bake |
| Emerald detection layers / HQ target adapter | Working on single scene | Must re-wire on additive load |
| Always-active distant AI | Not budgeted yet | High CPU risk → M8 budgets |
| Persistence of AI world state | Not implemented | Encounters reset unless saved (M8) |
| Female Zombie Emerald profile | Missing | Keep Playables greybox |

## Recommendations

1. Cap concurrent combat AI early (design budget before content density).
2. Region activation volumes for spawners.
3. Author Emerald Animation Profile for Female Zombie only after M5.5/M6 env pipeline stable.
4. Prefer existing humanoid/mutant packs that match grounded aesthetic; exclude fantasy skeletons.
