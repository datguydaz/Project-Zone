# Modular Environment Construction Standards (M6)

**Status:** Active for Outskirts productionizing.  
**Issues:** #41, #46, #47

## Scene organization

| Path | Role |
| --- | --- |
| `Assets/_ProjectZone/Scenes/PZ_Outskirts.unity` | Primary playable Outskirts scene |
| `Outskirts_Zones/` | Zone roots only |
| `Outskirts_NPCs/` | Spawned AI (not under zone Art) |
| Vendor packs | Stay under `Assets/05_Environments/…` — extract prefabs into zone `Art/` |

## Zone root contract

Every gameplay zone root must have:

1. `PzZoneMarker` (`Settlement` / `Checkpoint` / `Industrial` / `Underground`)
2. Children: `Greybox`, `Art`, `Collision`, `Spawns`, `Volumes`

| Folder | Contents |
| --- | --- |
| `Greybox` | Blockout cubes / layout only |
| `Art` | Approved modular prefabs (LOD Groups preferred) |
| `Collision` | Gameplay colliders / proxies (not dense scan meshes) |
| `Spawns` | Empty spawn markers |
| `Volumes` | Triggers (safe zone, transition, audio) |

Tools: `Tools → PROJECT ZONE → World → Validate Outskirts Zones` / `Rebuild Outskirts Greybox`.

## LOD / culling / collision (#47)

1. Dense props require a **LOD Group** before density increases.
2. Prefer simple mesh or box/capsule **collision proxies** under `Collision/`.
3. Do not rely on scan meshes for player/AI collision.
4. Mark static environment as **Static** for batching where possible.
5. Occlusion: enable after first art dress pass; measure before/after (PERF− until baseline exists).

## Loading (Phase A)

- Stay **single-scene Outskirts through M7**.
- Underground remains an in-scene entrance stub + transition volume.
- Additive underground / region loads deferred to later milestones (M13 plan).

## Visual style (blocky + photoreal)

See `docs/environment/ZONE_VISUAL_STYLE.md`.

- Prefer **prefab panel / industrial massing** (Post Soviet, russian_buildings, Hivemind industrial).
- Realism from **materials + decals + fog**, not dense sculpted geo outdoors.
- Break grids with **geometric clutter**, not curved fantasy architecture.
- Greybox Zone materials auto-apply on Outskirts (Art-safe; no menu).

## Asset policy

- Only **Approved / Evaluation** packs from `docs/assets/ASSET_REGISTRY.md`.
- Never drag full vendor demo scenes into `PZ_Outskirts`.
- Style exclusions in the registry beat convenience.
