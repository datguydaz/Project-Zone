# M5.5 — Technical Foundation Audit

**Date:** 2026-10-29  
**Scope:** Audit + documentation only. No M6 implementation.

## 1. Scenes & custom systems

| Scene | Role |
| --- | --- |
| `Assets/_ProjectZone/Scenes/PZ_Outskirts.unity` | Primary playable region |
| `Assets/_ProjectZone/Scenes/PZ_DamageAdapterTest.unity` | M2 combat sandbox |

Custom runtime (`Assets/_ProjectZone/Runtime/`): Combat, Expedition, Inventory, World bootstraps/adapters.

Vendor frameworks in use: Polymind HQ FPS, Emerald AI. Female Zombie greybox hostiles active on Outskirts.

## 2. Packages

URP 17.6, AI Navigation 2.0.16, Input System, Animation Rigging, Profile Analyzer, Performance Test Framework present. **No networking packages** (correct for this roadmap).

## 3. Assets

Dozens of large imported packs on disk (Hivemind, ALP, Abandoned World, etc.). Registry seeded: `docs/assets/ASSET_REGISTRY.md`. Many packs are **Evaluation/Deferred/Excluded** — not production-approved.

## 4. Git / LFS

- Git LFS installed; `.gitattributes` marks fbx/png/unitypackage as LFS.
- ~1344 LFS-tracked files observed historically.
- At least some newly imported FBX content still appears as raw binary in the working tree (not pointer) — **high repo-size risk**. Remediation required before further large imports.

## 5. Persistence

HQ + PZ adapters functional for M4/M5 loops. Cross-region additive loading **untested**. Save schema versioning **missing**. ALP `LevelManager` name clash mitigated in validator.

## 6. AI readiness

Single-scene Emerald + PZ zombies OK. Multi-region activation, AI budgets, and Emerald zombie profiles **not done**.

## 7. Performance

Targets documented. **No standalone FPS baseline recorded in this pass.** Profile Analyzer package available.

## 8. Deliverables checklist

| Deliverable | Path | Status |
| --- | --- | --- |
| Technical foundation audit | this file | Done (static) |
| Asset registry | `docs/assets/ASSET_REGISTRY.md` | Seeded |
| Performance baseline procedure | `docs/performance/BASELINE.md` | Done; measurements pending |
| World architecture proposal | `docs/architecture/WORLD_AND_SCENES.md` | Proposal |
| Asset pipeline standards | `docs/assets/PIPELINE_STANDARDS.md` | Draft |
| AI readiness | `docs/architecture/AI.md` | Done |
| Persistence assessment | `docs/architecture/PERSISTENCE.md` | Done |
| Technical decision log | `docs/architecture/TECHNICAL_DECISIONS.md` | Updated |
| Known issues | `docs/architecture/KNOWN_ISSUES.md` | Seeded |
| GitHub M5.5–M15 tracking | milestones/issues | Created via script |
| Hub sync | `website/` + sync script | After GitHub create |

## 9. Approval gate

**Stop before M6.** Developer should approve:

1. World loading approach (single-scene through M7 vs early additive underground).
2. Asset registry statuses (especially Excluded fantasy packs).
3. Reference hardware fields in BASELINE.md.
4. First standalone baseline run when convenient.
5. Authorization to begin M6 implementation.
