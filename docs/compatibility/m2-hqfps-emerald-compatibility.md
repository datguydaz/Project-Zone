# M2 Package Compatibility — HQ FPS Weapons 2.0 × Emerald AI

**Issue:** [#13](https://github.com/datguydaz/Project-Zone/issues/13)  
**Status:** Signed off (Play Mode PASS, 2026-10-29)  
**Scene:** `Assets/_ProjectZone/Scenes/PZ_DamageAdapterTest.unity`

## Stack under test

| Layer | Package / asset | Location | Notes |
|---|---|---|---|
| Engine | Unity 6 (`6000.6.7f1`) | `ProjectSettings/ProjectVersion.txt` | URP project |
| Render pipeline | URP `17.6.0` | `Packages/manifest.json` | Matches Unity 6 URP |
| FPS | HQ FPS Weapons 2.0 (PolymindGames) | `Assets/PolymindGames/` (`FPSCore` + `HQFPS`) | Vendor asset; do not edit for adapters |
| AI | Emerald AI (2024/2026 asset line in project) | `Assets/Emerald AI/` | Wiki shortcut still labeled 2024; project rule names Emerald AI 2026 |
| Project adapters | Project Zone combat bridges | `Assets/_ProjectZone/Runtime/Combat/` | Prefer adapters over vendor edits |

## Integration approach

Project Zone keeps vendor packages isolated and bridges them with thin adapters:

| Direction | Adapter | Role |
|---|---|---|
| HQ → Emerald | `EmeraldDamageReceiver` / `EmeraldDamageHitbox` | HQ `IDamageHandler` hits → `EmeraldHealth.Damage` |
| Emerald → HQ | `HqPlayerDamageBridge` (`EmeraldPlayerBridge`) | Emerald projectile/melee damage → HQ `IHealthManager` |
| Targeting | `EmeraldHqPlayerTargetAdapter` | Registers HQ player as Emerald combat target |
| Validation | `EmeraldCombatLoopValidator`, `EmeraldDeathHitReactionValidator`, `PackageCompatibilityValidator` | Play Mode checklists for M2 |

No Emerald or Polymind source changes are required for the verified combat loop.

## Verified in Play Mode (prior M2 issues)

| Capability | Issue | Result |
|---|---|---|
| HQ firearms damage Emerald AI | #8 | Confirmed |
| Emerald damage reaches HQ player health | #9 | Confirmed |
| Detection / pursuit of HQ player | #10 | Confirmed |
| Two-way ranged combat loop | #11 | Confirmed (melee deferred — no melee hostile in test scene) |
| AI hit reaction, AI death, player death, post-death ignore | #12 | Confirmed |

## Accepted vendor noise (non-blocking)

- Polymind `[SerializeReference]` warning: `CharacterIdleState` / missing `[Serializable]` on `CharacterMovementState` — vendor-side.
- Emerald UAC serialization analyzer warnings (`AnimatorStateInfo`, `Coroutine`, cover node types) — vendor-side.
- HQ death FX: `No active volume active in the scene` when dying in the integration scene — missing post-process volume, not a package clash.
- Unity 6 `EntityId` / `FindObjectsSortMode` deprecations — Project Zone combat scripts updated to current Find APIs.

## Deferred / out of M2 sign-off scope

- Melee hostile validation (blocked on scene content, not package APIs).
- Official Emerald “FPS Engine” vendor integration tutorial — Project Zone uses custom HQ adapters instead.
- Multiplayer / networking packages (explicitly postponed by project rules).

## Sign-off procedure

1. Open `PZ_DamageAdapterTest`.
2. On `DamageAdapterTestSetup`, temporarily enable **Package Compatibility Validation** (and other validators only if retesting).
3. Enter Play Mode.
4. Confirm Console shows `[ProjectZone][Compat] Package compatibility sign-off … Status: PASS`.
5. Spot-check: player can still damage an AI and take ranged damage (smoke of #11/#12 adapters).
6. Disable validators again after retest so later milestones stay quiet.

**Completed:** Play Mode PASS confirmed 2026-10-29; issue #13 and Milestone 2 closed.
