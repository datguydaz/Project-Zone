# Lighting, Terrain & Weather Strategy (M5.5 draft)

**Status:** Draft — choose before dense art integration (M6–M7).

## Principles

- One lighting approach for Outskirts.
- One fog system after benchmark (HAZE vs simple URP/volume fog).
- One foliage workflow (Toby candidate).
- Prefer baked/mixed lighting outdoors; limited realtime shadow casters.
- Weather only if cost is proven acceptable.

## Proposal

| System | Primary candidate | Fallback | Gate |
| --- | --- | --- | --- |
| Lighting | URP + overcast day / night presets | — | M7 |
| Fog | Evaluate HAZE vs lightweight fog | Simple fog if HAZE fails budget | M7 benchmark |
| Terrain | Unity Terrain or modular mesh kits (not both dense) | Mesh kits for industrial | M6 |
| Foliage | Toby Fredson (controlled density) | Static mesh props | M7 |
| Weather | Deferred until after atmosphere checkpoint | Light particles only | M7/M8 |

## Avoid

- Stacking HAZE + multiple third-party weather packs.
- Dropping vendor demo lighting settings assets into production scenes.
- Uncapped transparent foliage.
