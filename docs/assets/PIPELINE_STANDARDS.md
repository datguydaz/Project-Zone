# Asset Pipeline Standards (M5.5)

**Status:** Draft for approval.

## Import rules

1. No major Asset Store/Fab imports without written approval.
2. Prefer extracting production prefabs into controlled usage; do not keep vendor demos in playable scenes.
3. One primary foliage workflow; one primary fog approach after benchmark.
4. URP materials only in production content; convert via SRP converter when needed, then audit shaders.

## Folder conventions

| Path | Purpose |
| --- | --- |
| `Assets/_ProjectZone/` | All custom game code, production scenes, PZ-authored content |
| `Assets/_ProjectZone/Scenes/` | Playable scenes only |
| `Assets/<Vendor>/` | Immutable vendor packs (prefer adapters over edits) |
| `docs/assets/ASSET_REGISTRY.md` | Approval + risk tracking |

## Production scene policy

- Allowed: modular prefab instances from **Approved/Evaluation** packs.
- Forbidden: dragging full vendor demo scenes into `PZ_Outskirts`.
- Colliders: author gameplay collision; do not rely on dense scan meshes without proxies.
- LODs: require LOD Group on dense props before density increases.

## Performance gate for new art

1. Measure baseline.
2. Add art set.
3. Re-measure same route.
4. Record in `docs/performance/BENCHMARKS.md`.
5. Accept / optimize / defer / reject.
