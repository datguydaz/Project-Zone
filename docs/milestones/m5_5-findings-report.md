# M5.5 Findings Report

**Date:** 2026-10-29  
**Milestone:** 5.5 — Technical Foundation & Asset Readiness  
**Gate:** Stop — do not begin M6 without approval  

## What was done

1. Inspected repository, `Assets/_ProjectZone`, packages, GitHub milestones/issues, hub workflow (`hub-sync-and-deploy.yml` + `sync-github.mjs`).
2. Established M5.5–M15 GitHub tracking (already live) and hub catalog/sync mapping.
3. Seeded asset registry, pipeline standards, performance procedure, world/AI/persistence architecture drafts, backup policy.
4. Added **M1–M5 verification matrix** so reported completion is preserved while missing Play Mode/performance evidence is flagged separately.

## M1–M5 honesty

| Milestone | Keep as completed? | Separate flags |
| --- | --- | --- |
| M1–M5 | **Yes** — developer-reported Play Mode / process completion stands | **PERF−** project-wide (no standalone FPS baseline) |
| M2 melee | Completed milestone stays closed | **PM?** re-check with Female Zombie greybox optional |
| M3 art / real underground | Completed greybox stays closed | **DEFER** to M6–M7 |

See `docs/verification/M1-M5_VERIFICATION_MATRIX.md`.

## Technical findings

### Strengths

- Clear adapter pattern under `Assets/_ProjectZone/` (Combat, Inventory, Expedition, World).
- HQ FPS + Emerald remain primary frameworks; no networking packages installed.
- Playable `PZ_Outskirts` greybox loop exists (settlement → field hostiles → stash/contracts/death systems).
- Hub sync correctly treats open issues without completed work as **Planned**, not In Progress.
- Code-only backup path: git tags + `tools/Export-ProjectZoneBackup.ps1`.

### Risks

| Risk | Priority | Notes |
| --- | --- | --- |
| No measured FPS | Critical | Targets documented only |
| Huge untracked/imported vendor tree | Critical | Prefer registry-driven productionizing; LFS gaps |
| Name clashes / vendor Unity 6 APIs | Medium | ALP `LevelManager`; Toby `GetEntityId` patch |
| Single-scene only | Medium | Multi-region untested |
| Fantasy packs present | Medium | Marked Excluded — do not use |

### Asset shortlist for permanent consideration (Evaluation → likely Approve after M6/M7 benches)

**Environments:** ALP rustic / Post Soviet / Russian Buildings; Hivemind industrial; Abandoned World tunnels; Abandoned Road; Toby foliage (one workflow); Bridge Set / substation props.  
**Characters:** Female Zombie (active); Zombie Male AAB; NPC Postapocalypse; selective Adventure/Soldier/Mutant packs.  
**Atmosphere:** HAZE only after A/B vs simple fog.  
**Excluded:** Gothic/fantasy/carnival/skeletons/witch village/haunted farm.

## Deliverable checklist

| Deliverable | Status |
| --- | --- |
| Technical audit | Done (static) |
| Asset registry | Seeded |
| Performance baseline procedure | Done; **measurements pending** |
| World architecture proposal | Draft for approval |
| Asset pipeline standards | Draft for approval |
| AI readiness | Done |
| Persistence assessment | Done |
| Decision log | Updated |
| Verification matrix M1–M5 | Done |
| GitHub + hub tracking | Live (local snapshot synced; Pages via Actions on push) |

## Acceptance vs remaining developer actions

M5.5 **documentation/tracking** is complete enough to gate M6.  
Still required for full acceptance sign-off:

1. Approve pipeline standards + registry statuses.  
2. Fill reference PC in `docs/performance/BASELINE.md`.  
3. Optional: first standalone baseline.  
4. Choose loading approach (single-scene through M7 vs early additive underground).  
5. Explicit **approve M6** to start environment productionizing.

**Issues #33–#40 remain open** until you confirm the above (agents must not close them solely because docs exist).
