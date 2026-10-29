# Technical Decision Log

Append-only. Newest first.

## 2026-10-29 — M5.5–M15 roadmap established

- **Decision:** Track M5.5–M15 on GitHub + Development Hub; work milestones in order with approval gates.
- **Decision:** Performance-first doctrine: 120–150+ FPS typical, 100 FPS floor on reference config; standalone builds for acceptance.
- **Decision:** No networking packages during this roadmap. No major asset imports without approval.
- **Decision:** Keep HQ FPS + Emerald; extend via `Assets/_ProjectZone/` adapters.
- **Status:** Tracking setup + M5.5 audit docs. Awaiting approval to start M6.

## 2026-10-29 — Female Zombie greybox AI

- **Decision:** Use Playables clip driver instead of one-controller-per-clip swaps; freeze death on last frame.
- **Follow-up:** Emerald Animation Profile when env pipeline stable.

## Prior (M1–M5)

- HQ FPS + Emerald as primary frameworks.
- Outskirts greybox first playable region.
- Inventory/persistence and death/contracts as adapters on HQ systems.
