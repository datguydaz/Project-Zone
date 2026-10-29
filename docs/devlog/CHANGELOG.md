# PROJECT ZONE — Development Log

Only verified work belongs here. Do not invent activity.

## 2026-10-29 — M5.5 findings + M1–M5 verification honesty

**Kind:** documentation / process  
**Milestone:** M5.5  
**Public:** yes

Published M5.5 findings report and M1–M5 verification matrix: reported Play Mode completions preserved; missing standalone FPS and optional melee re-check flagged separately. M5.5 GitHub issues left open pending developer acceptance. M6 not started.

## 2026-10-29 — Code/scene backup policy adopted

**Kind:** process  
**Milestone:** M5.5  
**Public:** yes

Primary backup remains git commit/push + milestone tags. Added gitignored `Backups/` export script for `_ProjectZone`/docs/website/config only (excludes vendor packs).

## 2026-10-29 — M5.5–M15 roadmap tracking established

**Kind:** process / architecture  
**Milestone:** M5.5 Technical Foundation & Asset Readiness  
**Public:** yes

Created GitHub milestones/issues for M5.5–M15, seeded asset registry and performance/architecture docs, updated Development Hub catalog + sync mapping. M5.5 audit documentation started. No M6 implementation. Standalone FPS baseline not yet measured.

## 2026-10-29 — Female Zombie animation playback fixed

**Kind:** fix  
**Milestone:** M3 follow-up / combat greybox  
**Issue:** Outskirts NPCs  
**Public:** yes

Replaced one-controller-per-clip swaps with a Playables-driven Idle/Walk/Run/Attack/Death clip driver (`PzZombieAnimDriver`). Play Mode confirmed; death clip held on last frame (vendor import had loopTime enabled).

## 2026-10-29 — Female Zombie Outskirts hostiles wired

**Kind:** feature  
**Milestone:** M3 follow-up / combat greybox  
**Issue:** #11 (melee scene content) / Outskirts NPCs  
**Public:** yes

`OutskirtsNpcBootstrap` now spawns Female Zombie melee hostiles (chase + HQ damage volume) at Checkpoint and Industrial. Full Emerald Animation Profile for this pack remains a follow-up.

## 2026-10-29 — Milestone 5 completed (Death & Contracts)

**Kind:** milestone  
**Milestone:** M5 Death & Contracts  
**Issue:** #27–#32  
**Public:** yes

Play Mode confirmed hybrid death, death caches, wallet, contracts/rewards, and expedition loop on `PZ_Outskirts`. Issues #27–#32 and Milestone 5 closed.

## 2026-10-29 — M5 death & contracts adapters implemented (Play Mode pending)

**Kind:** feature  
**Milestone:** M5 Death & Contracts  
**Issue:** #27–#32  
**Public:** yes

Implemented hybrid safe-zone death, recoverable death caches, wallet, Settlement contract board/rewards, and expedition loop validator on `PZ_Outskirts`. Superseded by Milestone 5 completion entry above.

## 2026-10-29 — Milestone 4 completed (Inventory & Persistence)

**Kind:** milestone  
**Milestone:** M4 Inventory & Persistence  
**Issue:** #21–#26  
**Public:** yes

Play Mode confirmed on `PZ_Outskirts`: Settlement stash, loot containers, HQ Item Wheel + transfer panel, starting HQFPS_M1911 loadout, persistence adapters. Issues #21–#26 and Milestone 4 closed.

## 2026-10-29 — M4 inventory adapters implemented (Play Mode pending)

**Kind:** feature  
**Milestone:** M4 Inventory & Persistence  
**Issue:** #21–#26  
**Public:** yes

Implemented Project Zone adapters on top of HQ FPS inventory (loot containers, backpack UI, permanent stash, transfers, profile snapshots, SceneSaveHandler on `PZ_Outskirts`). Superseded by Milestone 4 completion entry above.

## 2026-10-29 — Milestone 3 completed (Outskirts)

**Kind:** milestone  
**Milestone:** M3 The Outskirts  
**Issue:** #19 / #20  
**Public:** yes

Play Mode confirmed NPCs/NavMesh (#19) and atmosphere systems (#20) on `PZ_Outskirts`. Issues #19–#20 and Milestone 3 closed. Fog/visual atmosphere and final NPC/prop art remain polish follow-ups, not blockers for M3 sign-off.

## 2026-10-29 — Underground entrance greybox verified

**Kind:** feature  
**Milestone:** M3 The Outskirts  
**Issue:** #18  
**Public:** yes

Play Mode confirmed underground entrance greybox in `PZ_Outskirts` (ramp, bunker/hatch, tunnel stub, transition marker). Issue #18 closed. Real underground load and art pass later.

## 2026-10-29 — Industrial building greybox verified

**Kind:** feature  
**Milestone:** M3 The Outskirts  
**Issue:** #17  
**Public:** yes

Play Mode confirmed industrial greybox in `PZ_Outskirts` (warehouse bay, dock, tanks, north gate). Issue #17 closed. Greybox stands in for a later art/prop pass.

## 2026-10-29 — Abandoned checkpoint greybox verified

**Kind:** feature  
**Milestone:** M3 The Outskirts  
**Issue:** #16  
**Public:** yes

Play Mode confirmed abandoned checkpoint greybox in `PZ_Outskirts` (road lane, ruined gate, booth, barriers/cover). Issue #16 closed. Greybox stands in for a later art/prop pass.

## 2026-10-29 — Safe settlement greybox verified

**Kind:** feature  
**Milestone:** M3 The Outskirts  
**Issue:** #15  
**Public:** yes

Play Mode confirmed safe settlement hub in `PZ_Outskirts` (perimeter, east gate, courtyard, habs, shed, watch post, SafeZoneVolume). Issue #15 closed. Greybox stands in for a later art/prop pass.

## 2026-10-29 — Outskirts region greybox verified

**Kind:** feature  
**Milestone:** M3 The Outskirts  
**Issue:** #14  
**Public:** yes

Play Mode confirmed `PZ_Outskirts` greybox: HQ GameMode spawn and walkable Settlement → Checkpoint → Industrial → UndergroundEntrance layout. Issue #14 closed.

## 2026-10-29 — Package compatibility signed off; M2 complete

**Kind:** feature / milestone  
**Milestone:** M2 Emerald AI Integration  
**Issue:** #13  
**Public:** yes

Play Mode `PackageCompatibilityValidator` reported PASS on Unity 6000.6.7f1 URP with HQ FPS Weapons 2.0 and Emerald AI adapters present. Issue #13 and Milestone 2 closed. M2 console validators default off to keep later milestones quiet.

## 2026-10-29 — Death and hit reactions verified

**Kind:** feature  
**Milestone:** M2 Emerald AI Integration  
**Issue:** #12  
**Public:** yes

Play Mode confirmed AI hit reactions, AI death to 0 HP, HQ player death, and post-death AI damage ignore on `PZ_DamageAdapterTest`. Issue #12 closed.

## 2026-10-29 — Ranged combat validation verified

**Kind:** feature  
**Milestone:** M2 Emerald AI Integration  
**Issue:** #11  
**Public:** yes

Play Mode confirmed two-way ranged combat: player damages Emerald hostiles and ranged AI damages the HQ player through Project Zone adapters. Issue #11 closed. Melee hostile not present in the integration scene yet — re-check when one is added. Polymind SerializeReference idle-state warning is vendor-side.

## 2026-10-29 — Public hub copy cleaned; combat validation harness added

**Kind:** note / feature  
**Milestone:** M2 Emerald AI Integration  
**Issue:** #11  
**Public:** yes

Public Development Hub activity/devlog copy no longer references vendor demo/template naming. Added `EmeraldCombatLoopValidator` for melee/ranged combat checklist tracking under issue #11.

## 2026-10-29 — Enemy detection and pursuit verified

**Kind:** feature  
**Milestone:** M2 Emerald AI Integration  
**Issue:** #10  
**Public:** yes

`EmeraldHqPlayerTargetAdapter` verified in Play Mode on `PZ_DamageAdapterTest`. HQ player registers as Emerald combat target; AIs report Enemy relation and pursue. TargetPositionModifier TransformSource assignment fixed (configure while inactive). Issue #10 closed.

## 2026-03-26 — Damage bridge playtest confirmed

**Kind:** feature  
**Milestone:** M2 Emerald AI Integration  
**Public:** yes

HQ FPS → Emerald and Emerald → HQ FPS damage adapters confirmed working in `PZ_DamageAdapterTest`. Flesh surface hits and player damage events verified by developer playtest. Remaining M2 scope: detection/pursuit coverage, broader combat validation, death/hit reactions, package compatibility sign-off.

## 2026-03-25 — Milestone 1 completed

**Kind:** milestone  
**Milestone:** M1 Unity & HQ FPS Foundation  
**Public:** yes

Unity 6 URP foundation, HQ FPS Weapons 2.0 integration, Firing Range validation, Git repository initialization, and permanent Cursor isolation rule confirmed complete.
