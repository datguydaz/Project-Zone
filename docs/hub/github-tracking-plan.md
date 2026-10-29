# GitHub Tracking Plan — PENDING APPROVAL

**Status:** Prepared locally. Remote milestones and issues have **not** been created.

This document is the review package for remote GitHub tracking. Execute only after explicit approval.

## Safety

- Repository must remain private until you decide otherwise.
- Do not push, publish Pages, or change visibility as part of this plan.
- Script: `website/scripts/create-github-tracking.mjs` (dry-run by default).
- Execute only with: `CONFIRM_GITHUB_TRACKING=1 node scripts/create-github-tracking.mjs --execute`

## Labels to ensure

`milestone`, `gameplay`, `ai`, `combat`, `inventory`, `world`, `multiplayer`, `ui`, `bug`, `enhancement`, `blocked`, `documentation`, `public`

For the private repo, hub sync publishes only issues labeled `public` (plus non-excluded).

## Milestones

| # | Title | Remote state | Hub status |
|---|-------|--------------|------------|
| 1 | Unity & HQ FPS Foundation | closed | Completed |
| 2 | Emerald AI Integration | open | In Progress |
| 3 | The Outskirts | open | Not Started |
| 4 | Inventory & Persistence | open | Not Started |
| 5 | Death & Contracts | open | Not Started |

## Issues

### M1 — Unity & HQ FPS Foundation (all closed; developer verified)

| Title | Labels | State |
|-------|--------|-------|
| Unity 6 project setup | milestone, documentation, public | closed |
| URP configuration | milestone, documentation, public | closed |
| HQ FPS Weapons 2.0 integration | milestone, gameplay, combat, public | closed |
| Weapon and movement validation | milestone, gameplay, combat, public | closed |
| Firing Range testing | milestone, gameplay, public | closed |
| Git repository initialization | milestone, documentation, public | closed |
| Permanent Cursor project rule | milestone, documentation, public | closed |

### M2 — Emerald AI Integration

Verified complete (closed):

| Title | Labels | State |
|-------|--------|-------|
| HQ FPS to Emerald damage adapter | milestone, ai, combat, public | closed |
| Emerald to HQ FPS damage adapter | milestone, ai, combat, public | closed |

Remaining (open — not invented as complete):

| Title | Labels | State |
|-------|--------|-------|
| Enemy detection and pursuit | milestone, ai, gameplay, public | open |
| Melee and ranged combat validation | milestone, ai, combat, public | open |
| Death and hit reaction testing | milestone, ai, combat, public | open |
| Package compatibility validation | milestone, ai, documentation, public | open |

### M3 — The Outskirts (all open)

| Title | Labels |
|-------|--------|
| First playable region layout | milestone, world, public |
| Safe settlement | milestone, world, public |
| Abandoned checkpoint | milestone, world, public |
| Industrial building | milestone, world, public |
| Underground entrance | milestone, world, public |
| NPC placement and navigation | milestone, ai, world, public |
| Environmental atmosphere | milestone, world, public |

### M4 — Inventory & Persistence (all open)

| Title | Labels |
|-------|--------|
| Loot containers | milestone, inventory, gameplay, public |
| Backpack UI | milestone, inventory, ui, public |
| Permanent stash | milestone, inventory, public |
| Inventory transfers | milestone, inventory, public |
| Save/load validation | milestone, inventory, public |
| Persistent equipment and items | milestone, inventory, gameplay, public |

### M5 — Death & Contracts (all open)

| Title | Labels |
|-------|--------|
| Hybrid death rules | milestone, gameplay, public |
| Recoverable death caches | milestone, gameplay, inventory, public |
| Currency | milestone, gameplay, public |
| Repeatable contracts | milestone, gameplay, public |
| Rewards and progression | milestone, gameplay, public |
| Complete expedition gameplay loop | milestone, gameplay, public |

## Manual GitHub steps (after approval)

1. Install/auth GitHub CLI: `winget install GitHub.cli` then `gh auth login`
2. Push hub commit when you approve push
3. Run dry-run: `cd website && node scripts/create-github-tracking.mjs`
4. Approve execute, then: `CONFIRM_GITHUB_TRACKING=1 node scripts/create-github-tracking.mjs --execute`
5. Confirm Actions can read issues (default `GITHUB_TOKEN` is enough for same-repo sync)
6. Keep `HUB_DEPLOY_ENABLED` unset until you want Pages published
