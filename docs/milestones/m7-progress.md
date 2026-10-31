# M7 Progress — Atmosphere, Lighting & Audio

**Started:** 2026-10-30  
**Status:** In progress  
**Gate:** Leave issues open until Play Mode / benchmark confirmation.

## Order

1. Unified lighting (#51) → overcast day (#52) → night (#53)
2. Fog A/B (#54) → foliage workflow (#55)
3. Audio zones (#56) → indoor/outdoor transitions (#57)
4. Weather eval (#58) → storytelling standards (#59) → checkpoint (#60)

## GitHub issues

| Issue | Title | Status |
| --- | --- | --- |
| #51 | Unified environmental lighting strategy | **Closed** |
| #52 | Overcast daytime preset | **Closed** (Stalzone day refs locked) |
| #53 | Nighttime preset | Open — Night v2 shipped; optional re-check |
| #54 | HAZE vs simple fog | **Closed** — HAZE primary (~270–300 FPS, look approved) |
| #55 | Primary foliage workflow | **Closed** — ~280 FPS with foliage under HAZE Overcast |
| #56 | Ambient audio zones | **Closed** — Outdoor ↔ Settlement confirmed |
| #57 | Indoor/outdoor atmosphere transitions | **Closed** — Outdoor ↔ Sheltered contrast confirmed |
| #58–#60 | Weather / storytelling / checkpoint | Open |

## Shipped (agents)

- Zone Overcast v6 + grey procedural skybox; Scene View ≈ Game look
- Atmosphere Settings (preset, fog backend, day grade)
- #54: HAZE locked primary via `PzFogApplicator` (SimpleLinear fallback)
- #55: Toby + Prefab Paint + TTFE locked; Play Mode ~280 FPS with foliage — **closed**
- #56: Outdoor ↔ Settlement ambient — **closed** (Play confirmed)
- #57: Outdoor / Sheltered / Indoor HAZE transitions — **closed** (Play confirmed)

## Needs developer

- #53: optional Night Play glance, then close
- Next: #58 lightweight weather eval (await approval)
