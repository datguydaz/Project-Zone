# PROJECT ZONE — Asset Registry

**Status:** Seeded under M5.5. Imported ≠ approved for production.  
**Engine:** Unity 6 URP.  
**Policy:** No major new imports without approval. Prefer adapters over editing vendor source.

Approval statuses: **Approved** | **Evaluation** | **Deferred** | **Excluded**

## Core frameworks

| Asset | Publisher | Folder | URP/U6 | Intended use | Visual fit | Perf risk | Status | Benchmark |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| HQ FPS Weapons 2.0 | Polymind Games | `Assets/PolymindGames/` | Compatible (in use) | Player, weapons, inventory | Core | Medium (FPS framework) | **Approved** | In active use; no formal FPS report yet |
| Emerald AI 2026 | Emerald AI | `Assets/Emerald AI/` | Compatible (in use) | NPC/enemy AI | Core | High if uncapped AI | **Approved** | Needs load budgets (M8) |

## Environments (priority order)

| Asset | Publisher | Folder | Intended use | Visual fit | Perf risk | Status |
| --- | --- | --- | --- | --- | --- | --- |
| ALP Rustic / Lake Coniferous / Country House | ALP Assets | `Assets/ALP_Assets/` | Rural settlement | High | Medium–High (large packs) | **Evaluation** |
| Green Bug Games Post Soviet World | Green Bug Games | `Assets/GreenBugGames/` | Rural/Soviet zone | High | Medium–High | **Evaluation** |
| Russian Buildings | (vendor) | `Assets/russian_buildings/` | Settlement buildings | High | Medium | **Evaluation** |
| Hivemind Industrial / Abandoned Factory / Post-Apoc Town / Sewers | Hivemind | `Assets/Hivemind/` | Industrial + underground | High | High (large scenes) | **Evaluation** |
| Abandoned World (Depot, Subway, Tunnels) | Abandoned World | `Assets/Abandoned World/` | Underground/depot | High | Medium–High | **Evaluation** |
| Abandoned Road | (vendor) | `Assets/Abandoned_Road/` | Roads/traversal | Medium–High | Medium | **Evaluation** |
| ScansFactory Cemetery / Urban / Warehouse | ScansFactory | `Assets/ScansFactory/` | Selective landmarks only | Medium–High | High (dense scans) | **Evaluation** (selective) |
| Toby Fredson Juniper + Rocky Hills + Foliage Engine | Toby Fredson | `Assets/Toby Fredson/` | Primary foliage candidate | High | High (vegetation) | **Evaluation** |
| Bridge Set | (vendor) | `Assets/BridgeSet/` | Connectivity props | Medium | Low–Medium | **Evaluation** |
| Transformer Substation Booth | (vendor) | `Assets/Transformer_substation_booth/` | Industrial prop | Medium | Low | **Evaluation** |
| Ultimate Apocalyptic Vehicles URP | (vendor) | `Assets/Ultimate Apocalyptic Vehicles-URP/` | Wreck props (selective) | Medium | Medium | **Deferred** |
| Top Down Post-Apocalyptic Pack | (vendor) | `Assets/Top_Down_Post-Apocalyptic_Pack/` | Reference / selective | Low (top-down) | Low | **Deferred** |
| Gothic Interior | (vendor) | `Assets/Gothic_Interior/` | — | Fantasy/gothic mismatch | Medium | **Excluded** (style) |
| Leartes Witch Village / The Aftermath | Leartes Studios | `Assets/LeartesStudios/` | Aftermath selective; Witch Village no | Mixed | High | Aftermath **Deferred**; Witch Village **Excluded** |
| Meshingun Apocalyptic Hospital / Carnival / Gothic Exterior | Meshingun Studio | `Assets/MeshingunStudio/` | Hospital selective; carnival/gothic no | Mixed | High | Hospital **Deferred**; others **Excluded** |
| Hivemind Fantasy Cemetery / Haunted Farm House | Hivemind | `Assets/Hivemind/` | — | Fantasy/horror mismatch | Medium | **Excluded** (style) |

## Characters / creatures

| Asset | Folder | Intended use | Fit | Perf risk | Status |
| --- | --- | --- | --- | --- | --- |
| Female Zombie Animations | `Assets/ZombieFemaleAnimations/` | Outskirts melee hostiles (active greybox) | High | Low–Medium | **Approved** (greybox); Emerald profile **Evaluation** |
| Zombie Male AAB | `Assets/ZombieMale_AAB/` | Hostile variant | High | Medium | **Evaluation** |
| NPC Postapocalypse 01 | `Assets/NPC_Postapocalypse_01/` | Settlement NPCs | High | Medium | **Evaluation** |
| Adventure Characters Pack | `Assets/Adventure_Characters_Pack/` | Selective NPCs | Medium | Medium | **Evaluation** |
| Soldier Future 03 | `Assets/Soldier_Future_03/` | Human hostiles if style fits | Medium | Medium | **Evaluation** |
| Mutant1 / Monsters Full Pack Vol 1 | `Assets/Mutant1/`, `Assets/Monsters Full Pack Vol 1/` | Mutants | Medium–High | Medium | **Evaluation** |
| Werewolf | `Assets/Werewolf/` | Mutant candidate | Medium | Medium | **Deferred** |
| Spiders / Biomass Webs / Medhue Wolf Spider / Snake3D | various | Anomaly fauna | Medium | Medium | **Deferred** |
| Skeleton Guard / Knight Modular | `Assets/Skeleton_*` | — | Fantasy mismatch | Medium | **Excluded** |
| DLNK characters | `Assets/_DLNK/` | Selective review | Unknown | Unknown | **Evaluation** |

## VFX / tools

| Asset | Folder | Intended use | Perf risk | Status |
| --- | --- | --- | --- | --- |
| HAZE Volumetric Fog URP | `Assets/HAZE - Volumetric Fog & Lighting for URP/` | Atmosphere candidate vs simple fog | High | **Evaluation** (must benchmark M7) |
| Decalery | `Assets/Decalery/` | Decals | Medium | **Evaluation** |
| PampelGames Gore Simulator | `Assets/PampelGames/` | Optional gore | High | **Deferred** |
| Hierarchy Designer | `Assets/Hierarchy Designer/` | Editor only | N/A | **Approved** (editor) |
| SRP Material Converter | `Assets/SRPMaterialConverter/` | Conversion utility | N/A | **Evaluation** |

## Notes

- Do **not** drop entire vendor demo scenes into production `PZ_Outskirts`.
- Prefer modular prefabs extracted into `_ProjectZone` adapters/content folders when productionizing.
- Conflicts found: ALP global `LevelManager` vs Polymind `LevelManager` (validator fully qualified). Toby demo `MirrorReflection` updated for Unity 6 `GetEntityId`.
