# Assets folder layout

Vendor packs keep their internal structure. Only the **root category** changes. Always move a pack **with its `.meta`** so Unity GUIDs stay valid.

| Order | Folder | Contents |
| --- | --- | --- |
| — | `Assets/_ProjectZone/` | Custom game code, production scenes, PZ-authored content (unchanged) |
| 1 | `Assets/01_System/` | Frameworks + engine project assets (PolymindGames, Emerald AI, TextMesh Pro, Settings, Samples, URP global settings, Input System actions) |
| 2 | `Assets/02_SFX/` | Dedicated audio / SFX packs (empty until imported) |
| 3 | `Assets/03_VFX/` | Fog, decals, gore, webs (HAZE, Decalery, PampelGames, BiomassWebs) |
| 4 | `Assets/04_Monsters/` | Creatures / hostiles (zombies, mutants, spiders, skeletons, snakes, etc.) |
| 5 | `Assets/05_Environments/` | Env / prefab packs + relocated vendor `*.lighting` demos under `_VendorLightingDemos/` |
| 6 | `Assets/06_Characters/` | Humanoid NPCs / soldiers / character kits |
| 7 | `Assets/07_Vehicles/` | Vehicle packs |
| 8 | `Assets/08_Tools/` | Editor utilities (Hierarchy Designer, SRP converter, Unity Readme) |

## Rules

1. Do not flatten vendor packs into shared Materials/Prefabs pools.
2. Prefer adapters in `_ProjectZone` over editing vendor source.
3. Hardcoded `AssetDatabase` / bootstrap paths in `_ProjectZone` must match this layout after any move.
4. Approval status still lives in `ASSET_REGISTRY.md` — folder ≠ approved for production.
