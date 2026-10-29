# World & Scene Architecture (M5.5 proposal)

**Status:** Proposal — requires approval before M6 implementation.  
**Current production scenes:** `PZ_Outskirts.unity`, `PZ_DamageAdapterTest.unity` (combat sandbox).

## Goals

- Handcrafted interconnected exclusion-zone regions.
- Stable 120–150+ FPS; no always-loaded megascene of every vendor demo.
- Persist inventory/stash/contracts/wallet across loads (HQ + PZ adapters).
- Co-op-ready later without networking packages now.

## Recommended approach

### Production content

- Keep gameplay scenes under `Assets/_ProjectZone/Scenes/`.
- Build locations as modular prefab kits / greybox zones first, then dress with **approved** extracted prefabs.
- Never parent entire vendor demo scenes into `PZ_Outskirts`.

### Loading strategy (proposal)

1. **Phase A (M6):** Single primary Outskirts scene with streaming-friendly structure (zones as root objects); optional additive underground stub when player approaches entrance.
2. **Phase B (M13):** Region additive loads (`Settlement`, `FieldNorth`, `Industrial`, `Underground`) with a thin bootstrap scene holding player + managers.
3. Persist via existing HQ `SceneSaveHandler` / `LevelManager` + PZ profile files (`pz_wallet`, stash, snapshots). Version keys documented under persistence audit.

### Zone map (Outskirts)

| Zone | Role | Asset preference |
| --- | --- | --- |
| Settlement | Safe hub, stash, contracts | ALP / Post Soviet / Russian Buildings |
| Checkpoint | Contested mid | Abandoned Road + modular props |
| Industrial | Combat/loot | Hivemind industrial / Abandoned Depot |
| Underground | Verticality / risk | Abandoned World tunnels / Sewers (selective) |

## AI / NavMesh

- Bake NavMesh per loaded scene/region.
- Emerald agents only active inside activation radius (formalize M8).
- Female Zombie greybox remains until Emerald Animation Profile approved.

## Open decisions for approval

1. Stay single-scene Outskirts through M7, or start additive underground in M6?
2. Primary foliage: Toby Fredson vs lighter mesh foliage?
3. Primary fog: HAZE vs URP/simple volume fog after benchmark?
