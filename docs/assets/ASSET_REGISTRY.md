# PROJECT ZONE — Asset Registry

**Status:** Seeded under M5.5. Imported ≠ approved for production.  
**Engine:** Unity 6 URP.  
**Policy:** No major new imports without approval. Prefer adapters over editing vendor source.  
**Layout:** See `FOLDER_LAYOUT.md` (01_System … 08_Tools).

Approval statuses: **Approved** | **Evaluation** | **Deferred** | **Excluded**

## Core frameworks

| Asset | Publisher | Folder | URP/U6 | Intended use | Visual fit | Perf risk | Status | Benchmark |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| HQ FPS Weapons 2.0 | Polymind Games | `Assets/01_System/PolymindGames/` | Compatible (in use) | Player, weapons, inventory | Core | Medium (FPS framework) | **Approved** | In active use; no formal FPS report yet |
| Emerald AI 2026 | Emerald AI | `Assets/01_System/Emerald AI/` | Compatible (in use) | NPC/enemy AI | Core | High if uncapped AI | **Approved** | Needs load budgets (M8) |

## Environments (priority order)

Style lock: **blocky prefab massing + photoreal surfaces** — `docs/environment/ZONE_VISUAL_STYLE.md`.

| Asset | Publisher | Folder | Intended use | Visual fit | Massing | Perf risk | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| ALP Rustic / Lake Coniferous / Country House | ALP Assets | `Assets/05_Environments/ALP_Assets/` | Rural accents / foliage-adjacent | High | Mixed (use selectively; avoid ornate) | Medium–High (large packs) | **Evaluation** |
| Green Bug Games Post Soviet World | Green Bug Games | `Assets/05_Environments/PostSovietWorld/` | Primary settlement massing | High | **Primary** (panel / slab) | Medium–High | **Evaluation** |
| Russian Buildings | (vendor) | `Assets/05_Environments/russian_buildings/` | Settlement buildings | High | **Primary** | Medium | **Evaluation** |
| Hivemind Industrial / Abandoned Factory / Post-Apoc Town / Sewers | Hivemind | `Assets/05_Environments/Hivemind/` | Industrial + underground | High | **Primary** industrial | High (large scenes) | **Evaluation** |
| Abandoned World (Depot, Subway, Tunnels) | Abandoned World | `Assets/05_Environments/Abandoned World/` | Underground/depot | High | Strong (slab / tunnel) | Medium–High | **Evaluation** |
| Abandoned Road | (vendor) | `Assets/05_Environments/Abandoned_Road/` | Roads/traversal | Medium–High | Strong | Medium | **Evaluation** |
| ScansFactory Cemetery / Urban / Warehouse | ScansFactory | `Assets/05_Environments/ScansFactory/` | Selective landmarks only | Medium–High | Too dense outdoors — selective | High (dense scans) | **Evaluation** (selective) |
| Toby Fredson Juniper + Rocky Hills + Foliage Engine | Toby Fredson | `Assets/05_Environments/Toby Fredson/` | **Primary foliage** (#55) via Prefab Paint + TTFE | High | Softens yards (not buildings) | High (vegetation) | **Approved** (workflow) |
| Bridge Set | (vendor) | `Assets/05_Environments/BridgeSet/` | Connectivity props | Medium | Good | Low–Medium | **Evaluation** |
| Transformer Substation Booth | (vendor) | `Assets/05_Environments/Transformer_substation_booth/` | Industrial prop | Medium | Good (boxy) | Low | **Evaluation** |
| Ultimate Apocalyptic Vehicles URP | (vendor) | `Assets/07_Vehicles/Ultimate Apocalyptic Vehicles-URP/` | Wreck props (selective) | Medium | Wreck clutter | Medium | **Deferred** |
| Top Down Post-Apocalyptic Pack | (vendor) | `Assets/05_Environments/Top_Down_Post-Apocalyptic_Pack/` | Reference / selective | Low (top-down) | Poor (scale) | Low | **Deferred** |
| Gothic Interior | (vendor) | `Assets/05_Environments/Gothic_Interior/` | — | Fantasy/gothic mismatch | Excluded | Medium | **Excluded** (style) |
| Leartes Witch Village / The Aftermath | Leartes Studios | `Assets/05_Environments/LeartesStudios/` | Aftermath selective; Witch Village no | Mixed | Aftermath selective | High | Aftermath **Deferred**; Witch Village **Excluded** |
| Meshingun Apocalyptic Hospital / Carnival / Gothic Exterior | Meshingun Studio | `Assets/05_Environments/MeshingunStudio/` | Hospital selective; carnival/gothic no | Mixed | Hospital selective | High | Hospital **Deferred**; others **Excluded** |
| Hivemind Fantasy Cemetery / Haunted Farm House | Hivemind | `Assets/05_Environments/Hivemind/` | — | Fantasy/horror mismatch | Excluded | Medium | **Excluded** (style) |
| Playground Apocalypse | (vendor) | `Assets/05_Environments/PlaygroundApocalypse/` | Selective props | Medium | Mixed | Medium | **Evaluation** |

## Characters / creatures

| Asset | Folder | Intended use | Fit | Perf risk | Status |
| --- | --- | --- | --- | --- | --- |
| Female Zombie Animations | `Assets/04_Monsters/ZombieFemaleAnimations/` | Outskirts melee hostiles (active greybox) | High | Low–Medium | **Approved** (greybox); Emerald profile **Evaluation** |
| Zombie Male AAB | `Assets/04_Monsters/ZombieMale_AAB/` | Hostile variant | High | Medium | **Evaluation** |
| NPC Postapocalypse 01 | `Assets/06_Characters/NPC_Postapocalypse_01/` | Settlement NPCs | High | Medium | **Evaluation** |
| Adventure Characters Pack | `Assets/06_Characters/Adventure_Characters_Pack/` | Selective NPCs | Medium | Medium | **Evaluation** |
| Soldier Future 03 | `Assets/06_Characters/Soldier_Future_03/` | Human hostiles if style fits | Medium | Medium | **Evaluation** |
| Mutant1 / Monsters Full Pack Vol 1 | `Assets/04_Monsters/Mutant1/`, `Assets/04_Monsters/Monsters Full Pack Vol 1/` | Mutants | Medium–High | Medium | **Evaluation** |
| Werewolf | `Assets/04_Monsters/Werewolf/` | Mutant candidate | Medium | Medium | **Deferred** |
| Spiders / Biomass Webs / Medhue Wolf Spider / Snake3D | `04_Monsters` + `03_VFX/_BiomassWebs` | Anomaly fauna | Medium | Medium | **Deferred** |
| Skeleton Guard / Knight Modular | `Assets/04_Monsters/Skeleton_*` | — | Fantasy mismatch | Medium | **Excluded** |
| DLNK characters | `Assets/06_Characters/_DLNK/` | Selective review | Unknown | Unknown | **Evaluation** |

## VFX / tools / SFX

| Asset | Folder | Intended use | Perf risk | Status |
| --- | --- | --- | --- | --- |
| HAZE Volumetric Fog URP | `Assets/03_VFX/HAZE - Volumetric Fog & Lighting for URP/` | Atmosphere candidate vs simple fog | High | **Evaluation** (must benchmark M7) |
| Decalery | `Assets/03_VFX/Decalery/` | Decals | Medium | **Evaluation** |
| PampelGames Gore Simulator | `Assets/03_VFX/PampelGames/` | Optional gore | High | **Deferred** |
| Hierarchy Designer | `Assets/08_Tools/Hierarchy Designer/` | Editor only | N/A | **Approved** (editor) |
| SRP Material Converter | `Assets/08_Tools/SRPMaterialConverter/` | Conversion utility | N/A | **Evaluation** |
| Dedicated SFX packs | `Assets/02_SFX/` | Audio | — | Empty (placeholder) |

## Notes

- Do **not** drop entire vendor demo scenes into production `PZ_Outskirts`.
- Prefer modular prefabs extracted into `_ProjectZone` adapters/content folders when productionizing.
- Conflicts found: ALP global `LevelManager` vs Polymind `LevelManager` (validator fully qualified). Toby demo `MirrorReflection` updated for Unity 6 `GetEntityId`.
