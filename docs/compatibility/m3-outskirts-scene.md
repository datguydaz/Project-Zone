# M3 Scene Notes — The Outskirts

**Issue:** [#14](https://github.com/datguydaz/Project-Zone/issues/14) (closed — Play Mode verified)  
**Primary scene:** `Assets/_ProjectZone/Scenes/PZ_Outskirts.unity`

## Scene roles

| Scene | Role |
|---|---|
| `PZ_Outskirts` | Milestone 3 playable region (greybox → settlement content) |
| `PZ_DamageAdapterTest` | Milestone 2 combat / Emerald AI regression sandbox |
| `HQFPS_FiringRange` | Vendor demo — do not expand for world content |

## Layout contents (#14 / #15)

Hierarchy under `Outskirts Region` after bootstrap:

- `Ground`
- `Outskirts_Zones`
  - `Zone_Settlement` (#15 safe hub)
    - Courtyard, perimeter walls, east gate pillars
    - `Hab_A`, `Hab_B`, `SupplyShed`, `WatchPost`
    - `SafeZoneVolume` (trigger marker for later systems)
  - `Zone_Checkpoint` (#16 abandoned)
    - Road lane, ruined west gate, fallen boom
    - `GuardBooth`, sandbags, jersey barriers, overturned cover
  - `Zone_Industrial` (#17)
    - Warehouse shell (open bay), loading dock, office annex
    - Tank farm, pipe run, north gate toward Underground
  - `Zone_UndergroundEntrance` (#18)
    - Approach ramp, bunker lip/wings, hatch
    - Short tunnel stub + `UndergroundTransitionMarker` (future load/interact)
  - connecting `Path_*` cubes

Player spawn: HQ `GameMode` prefab at Settlement approach `(0, 2.2, -8)`.

NavMesh: `NavMesh Surface` on `Outskirts Region`. `OutskirtsNpcBootstrap` bakes on Play Mode start.

NPCs (#19): under `Outskirts_NPCs` — settlement civilian NavMesh wanderer; Emerald hostiles at Checkpoint + Industrial (Example Summoned AI prefab). HQ target adapter wires automatically.

Atmosphere (#20): `OutskirtsAtmosphereBootstrap` — dusk directional light, denser fog, global URP volume (exposure/contrast/vignette), dust particle hints, ambient `AudioSource` (clip assign later).

Layout version is stamped on `OutskirtsGreyboxBootstrap`; bump forces rebuild when opening the scene.

## Rebuild

- Context menu on `OutskirtsGreyboxBootstrap`: **Rebuild Outskirts Greybox**
- Or menu: **Project Zone → Scenes → Build Outskirts Region Scene** (full scene rewrite via Editor)
