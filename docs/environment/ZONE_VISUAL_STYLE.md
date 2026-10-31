# Zone Visual Style — Blocky Massing + Photoreal Surface

**Status:** Active (M7 — locks look before dense Outskirts art dress)  
**Related:** `#59` storytelling · `MODULAR_STANDARDS.md` · `LIGHTING_WEATHER_STRATEGY.md`  
**Reference push:** Stalzone / Stalcraft daytime overcast (developer gameplay refs) — flat grey sky, soft sun-in-cloud, muddy desat, readable world. Not pixel art, not low-poly toy, not crushed night.

## One-line rule

**Prefabricated Soviet geometry + photographic materials + Zone atmosphere.**  
Silhouettes stay chunky; realism lives in dirt, rust, wet concrete, fog, and grade — not in mesh denseness.

## Do

| Lever | Guidance |
| --- | --- |
| Massing | Rectangular volumes, flat slabs, repeated panels, hard corners, clear floors/roofs |
| Modules | Panel buildings, warehouses, booths, jersey barriers, tanks, pipe runs, fences |
| Materials | High-quality PBR concrete, painted/oxidized metal, asphalt, packed dirt |
| Detail | Decals (stains, leaks, posters), edge wear, puddles, cables, clutter kits |
| Clutter | Crates, barrels, scrap, sandbags — still geometric |
| Scale | Real door/rail/prop heights; only architecture massing is simplified |
| Atmosphere | Dense fog + desat + pale sun (M7 profiles) so blocks read as Zone silhouettes |

## Don’t

- Pixel / voxel / Minecraft-like cubes as final art
- Ornate fantasy, gothic, carnival, witch-village kits
- Hyper-scanned every-brick density outdoors (ScansFactory = selective landmarks only)
- Soft organic “Unreal showcase” buildings as primary settlement massing
- Bright green lawn greybox reading as farmland instead of Zone yard

## Pack priority (style fit)

| Fit | Packs | Use |
| --- | --- | --- |
| **Primary massing** | Post Soviet World, russian_buildings, Hivemind industrial / factory / post-apoc town | Settlement + industrial shells |
| **Traversal / road** | Abandoned Road, Bridge Set, Transformer booth | Checkpoint + connectors |
| **Underground** | Abandoned World, Hivemind sewers | Entrance + later tunnels |
| **Foliage** | Toby Fredson (#55 locked) | Soften hard yards; dry/autumn via Prefab Paint + TTFE |
| **Selective only** | ScansFactory, Aftermath, Hospital, Playground Apocalypse | Landmark or interior accents |
| **Excluded** | Gothic, Witch Village, carnival, fantasy cemetery/farm | Style mismatch |

## Dress workflow (developer-owned placement)

1. Keep **Greybox** for layout; put final pieces under each zone’s `Art/`.
2. Prefer modular prefabs with **LOD Groups** before density increases.
3. Break the grid with **clutter**, not with curved fantasy roofs.
4. Apply Zone shared materials / vendor PBR — avoid unique shiny demo mats.
5. Rely on **Zone Overcast** (auto-apply) for the photographic grade; do not fight it with sunny vendor volumes.

## Greybox → style preview

Auto on Outskirts open / script reload / Play / greybox rebuild (Art untouched).  
Shared mats: `Assets/_ProjectZone/Art/Materials/Zone/`.

## Acceptance sniff test

With fog + grade on: distant buildings read as **prefab concrete blocks in murk**.  
Without fog: same buildings still look like **photoreal Soviet yard**, not toys or voxels.
