# World & Scene Architecture

**Status:** M6 Phase A — approved (developer started M6 2026-10-30).  
**Current production scenes:** `PZ_Outskirts.unity`, `PZ_DamageAdapterTest.unity` (combat sandbox).

## Goals

- Handcrafted interconnected exclusion-zone regions.
- Stable 120–150+ FPS; no always-loaded megascene of every vendor demo.
- Persist inventory/stash/contracts/wallet across loads (HQ + PZ adapters).
- Co-op-ready later without networking packages now.

## Loading strategy

1. **Phase A (M6–M7):** Single primary Outskirts scene with streaming-friendly zone roots; underground stays an in-scene stub + transition volume.
2. **Phase B (M13):** Region additive loads (`Settlement`, `FieldNorth`, `Industrial`, `Underground`) with a thin bootstrap scene holding player + managers.
3. Persist via existing HQ `SceneSaveHandler` / `LevelManager` + PZ profile files (`pz_wallet`, stash, snapshots).

## Zone map (Outskirts)

| Zone | Role | Hierarchy | Asset preference |
| --- | --- | --- | --- |
| Settlement | Safe hub, stash, contracts | `Zone_Settlement` + `PzZoneMarker` | ALP / Post Soviet / Russian Buildings |
| Checkpoint | Contested mid | `Zone_Checkpoint` | Abandoned Road + modular props |
| Industrial | Combat/loot | `Zone_Industrial` | Hivemind industrial / Abandoned Depot |
| Underground | Entrance stub (Phase A) | `Zone_UndergroundEntrance` | Abandoned World tunnels (selective later) |

Standards: `docs/environment/MODULAR_STANDARDS.md`.

## NavMesh / transitions (#48)

- Bake `NavMeshSurface` on the Outskirts root after greybox/art changes.
- Menu: `OutskirtsNpcBootstrap` context → Bake Outskirts NavMesh.
- Agents stay in the loaded Outskirts scene for Phase A (no region unload yet).
- Transition volume `UndergroundTransitionMarker` reserves a future additive load point — do not wire SceneManager load until Phase B.

## Persistence across regions (#49)

Phase A (single scene): existing HQ save + PZ stash/wallet/snapshots already cover Outskirts.

When Phase B starts:

- Keep player + managers in bootstrap scene.
- Save before unload; restore containers after additive load.
- Version keys: document any new zone-scoped ids under persistence audit.

## AI

- Emerald agents only active inside activation radius (formalize M8).
- Female Zombie greybox remains until Emerald Animation Profile approved.

## Open decisions (carry to M7)

1. Primary foliage: **Locked** — Toby + Prefab Paint (`docs/environment/FOLIAGE_WORKFLOW.md`). Density Play confirm still open on #55.
2. Primary fog: **Locked** — HAZE volumetric (#54); SimpleLinear fallback.
