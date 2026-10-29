# Persistence Architecture (M5.5 assessment)

## Existing systems (preserve)

| Data | Mechanism | Notes |
| --- | --- | --- |
| HQ inventory / scene objects | Polymind `SceneSaveHandler` + `LevelManager` | F7 validator path uses `PolymindGames.LevelManager` |
| Player inventory snapshot | `PzPlayerInventorySnapshot` | F5/F9 |
| Permanent stash | `PzPermanentStash` profile file | F6 |
| Wallet | `PzWallet` (`pz_wallet.sav`) | M5 |
| Contracts / expedition phase | PZ expedition services | M5 |
| Death caches | `PzDeathCacheService` | M5 |

## Risks

1. **ALP `LevelManager` name clash** — global MonoBehaviour shadows Polymind; always fully qualify.
2. **Save schema versioning** — PZ profile files lack explicit version headers; add before M10 economy expansion.
3. **Cross-region loads** — untested; additive load must not double-create bootstrap services.
4. **Git LFS** — `.gitattributes` declares LFS for fbx/png/etc., but some imported binaries may still be plain Git objects (audit found Female Zombie FBX as raw binary header). Repo size / clone risk is **high**.

## Recommendations

1. Add `schemaVersion` to all PZ save DTOs.
2. Single bootstrap ownership for wallet/stash/expedition (`DontDestroyOnLoad` or scene-unique).
3. Document save slot policy (HQ slot 0 vs PZ files).
4. Complete LFS migration plan for oversized non-pointer assets before more imports.
