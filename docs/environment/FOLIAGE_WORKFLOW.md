# Primary Foliage Workflow (#55)

**Status:** Locked + Play confirmed 2026-10-31 (~280 FPS with foliage under HAZE)  
**Issue:** #55 (closed)  
**Rule:** One foliage stack for Outskirts — no dual Terrain-detail + mesh paint for the same ground cover.

## Decision

| Lever | Choice |
| --- | --- |
| Vendor | **Toby Fredson** — Rocky Hills (Whitebark Pine) vegetation + Juniper pack as needed |
| Engine | **Toby Foliage Engine (TTFE)** wind/global controller |
| Placement | **PROJECT ZONE Prefab Paint** (`Tools → PROJECT ZONE → Prefab Paint Tool`) |
| Hierarchy | Zone `Art/` category folders, or fallback `PZ_PaintedFoliage` |
| Not primary | ALP nature as main foliage, Polybrush, Unity Terrain grass painting, vendor demo scenes |

## Approved prefab roots

- `Assets/05_Environments/Toby Fredson/Rocky Hills Environment - Whitebark Pine/RHEWP_Demo/Prefabs/Prefabs_Vegetation/`
  - `PV_Trees/` — silhouette / windbreak
  - `PV_Bushes/` — yard break-up
  - `PV_Grass/` — prefer **Dry** / tall dry plants (Stalzone murk)
- Juniper pack: selective bushes/trees only when Rocky Hills isn’t enough
- **Skip** `Inst Terrain Details` / `(TD)` prefabs for mesh paint (Terrain-only path)

## Density / perf rules

From Atmosphere Settings → World (and Prefab Paint clamps):

| Category | Min spacing (default) | Notes |
| --- | --- | --- |
| Tree | 2.5 m | LOD Group required |
| Bush | 1.0 m | LOD preferred |
| Grass | 0.45 m | Keep sparse near combat lanes |
| Flower | 0.6 m | Optional; desat grade hides color pop |

- Soft cap: **≤ ~800** painted props per Outskirts zone before a FPS pass.
- Prefer autumn / dry variants; HAZE + Zone Overcast sell grit more than denser leaves.
- Falling-leaf FX: optional, one system only (TTFE particles) — don’t stack multiple leaf rains.

## Scene setup

1. `Tools → PROJECT ZONE → Foliage → Ensure TTFE Global Controller`
2. `Tools → PROJECT ZONE → Foliage → Seed Paint Palette (Toby Rocky Hills)`
3. Paint under zone `Art/` (or organize via Organize Painted Hierarchy)
4. Play Mode: Settlement → Checkpoint path — confirm FPS floor diagnostic

## Explicit non-goals (#55)

- Full Outskirts foliage dress (still developer-owned env art after M7)
- Replacing greybox with forest showcase
- Enabling HAZE density volumes just for grass
