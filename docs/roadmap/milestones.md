# PROJECT ZONE — Milestone Roadmap

Authoritative **live** progress comes from GitHub Milestones + Issues after sync.
`website/data/hub-catalog.json` is the local planning catalog and must not be shown as live verified progress.

**Performance doctrine:** typical 120–150+ FPS; 100 FPS floor on documented reference hardware (standalone builds). Do not claim targets met until measured.

**Verification honesty:** M1–M5 stay **Completed** as developer-reported. Missing standalone FPS and any re-checks are tracked separately in `docs/verification/M1-M5_VERIFICATION_MATRIX.md` — never invent completion from code presence alone.

## Milestone 1 — Unity & HQ FPS Foundation

- **Status:** Completed (reported)
- Unity 6 / URP / HQ FPS Weapons 2.0 foundation verified
- **Gaps:** PERF− (no project FPS baseline)

## Milestone 2 — Emerald AI Integration

- **Status:** Completed (reported)
- HQ↔Emerald combat adapters verified; Female Zombie melee greybox added later on Outskirts
- **Gaps:** PERF−; optional PM? melee re-check with zombies (do not auto-reopen)

## Milestone 3 — The Outskirts

- **Status:** Completed (reported)
- Greybox region (`PZ_Outskirts`) verified; art/underground load polish deferred to M6+
- **Gaps:** PERF−; art/real underground = DEFER

## Milestone 4 — Inventory & Persistence

- **Status:** Completed (reported)
- Loot, stash, transfers, snapshots, starting M1911 verified
- **Gaps:** PERF−

## Milestone 5 — Death & Contracts

- **Status:** Completed (reported)
- Hybrid death, caches, wallet, contracts, expedition loop verified
- **Gaps:** PERF−

## Milestone 5.5 — Technical Foundation & Asset Readiness

- **Status:** Findings ready — awaiting developer acceptance / M6 approval
- **Gate:** Stop for approval before M6
- Report: `docs/milestones/m5_5-findings-report.md`
- Issues #33–#40 remain open until you confirm acceptance (docs alone ≠ close)
- Audit scenes/packages/custom systems
- Region/scene-loading architecture proposal
- Asset import conventions + registry
- Lighting/terrain/weather strategy
- Performance targets + profiling procedure (measurements still pending)
- Emerald multi-region readiness
- Git LFS / save versioning / licensing audit
- Technical decision log + roadmap sync
- M1–M5 verification matrix

## Milestone 6 — World Architecture & Environment Pipeline

- **Status:** Planned
- Settlement, checkpoint, industrial, underground production layout
- Modular env standards, LOD/culling, NavMesh, persistence, traversal benchmarks

## Milestone 7 — Atmosphere, Lighting & Audio

- **Status:** Planned
- Unified lighting, day/night, HAZE vs simple fog, foliage workflow, audio zones, atmosphere checkpoint

## Milestone 8 — Living World & Dynamic Encounters

- **Status:** Planned
- Encounter management, human/mutant spawns, AI budgets, activation distances, load profiling

## Milestone 9 — Factions, NPCs & Settlements

- **Status:** Planned
- Factions, traders, light dialogue, reputation, settlement NPC budgets

## Milestone 10 — Loot, Economy & Equipment Progression

- **Status:** Planned
- Loot/economy/repair/attachments/armor on HQ FPS; anti-dupe; save compatibility

## Milestone 11 — Contracts, Exploration & Anomalies

- **Status:** Planned
- Expanded contracts, POIs, anomaly prototype, hazard counterplay

## Milestone 12 — Combat, Mutants & Survival

- **Status:** Planned
- Human/mutant variety, feedback, optional survival eval, combat stress tests

## Milestone 13 — World Expansion & Optimization

- **Status:** Planned
- More locations, connectivity, vegetation/lighting/AI/memory optimization

## Milestone 14 — UI/UX, Accessibility & Polish

- **Status:** Planned
- Inventory/loot/contracts/map/HUD, settings, rebinding, accessibility

## Milestone 15 — Vertical Slice & Alpha Readiness

- **Status:** Planned
- Feature freeze, critical bugs, full loop + performance validation, internal alpha build
