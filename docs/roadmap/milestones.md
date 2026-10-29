# PROJECT ZONE — Milestone Roadmap

Authoritative **live** progress comes from GitHub Milestones + Issues after sync.
`website/data/hub-catalog.json` is the local planning catalog and must not be shown as live verified progress.

## Milestone 1 — Unity & HQ FPS Foundation

- **Status:** Completed (developer confirmed)
- Unity 6 project setup
- URP configuration
- HQ FPS Weapons 2.0 integration
- Weapon and movement validation
- Firing Range testing
- Git repository initialization
- Permanent Cursor project rule

## Milestone 2 — Emerald AI Integration

- **Status:** Completed (developer confirmed)
- HQ FPS to Emerald damage adapter — verified in play mode (#8)
- Emerald to HQ FPS damage adapter — verified in play mode (#9)
- Enemy detection and pursuit — verified in play mode (#10)
- Melee and ranged combat validation — ranged verified (#11); Female Zombie melee hostiles added on Outskirts (Play Mode re-check for melee checkbox)
- Death and hit reaction testing — verified in play mode (#12)
- Package compatibility validation — verified in play mode (#13)

## Milestone 3 — The Outskirts

- **Status:** Completed (developer confirmed)
- **Scene:** `Assets/_ProjectZone/Scenes/PZ_Outskirts.unity` (keep `PZ_DamageAdapterTest` as M2 combat sandbox; leave `HQFPS_FiringRange` as vendor demo)
- First playable region layout — verified in play mode (#14)
- Safe settlement — verified greybox hub in play mode (#15); art/prop pass later
- Abandoned checkpoint — verified greybox in play mode (#16); art/prop pass later
- Industrial building — verified greybox in play mode (#17); art/prop pass later
- Underground entrance — verified greybox in play mode (#18); real transition/art later
- NPC placement and navigation — verified in play mode (#19); final NPC art/behavior later
- Environmental atmosphere — systems verified in play mode (#20); fog/look polish later

## Milestone 4 — Inventory & Persistence

- **Status:** Completed (Play Mode confirmed)
- Loot containers — Outskirts loot markers + HQ containers (#21)
- Backpack UI — HQ Item Wheel (MMB) + loot/stash transfer panel (#22)
- Permanent stash — Settlement courtyard stash + profile save (#23)
- Inventory transfers — HQ ItemTransfering dual panel (#24)
- Save/load validation — snapshots, stash file, SceneSaveHandler (#25)
- Persistent equipment and items — holster/backpack snapshot + starting M1911 (#26)

## Milestone 5 — Death & Contracts

- **Status:** Completed (Play Mode confirmed)
- Hybrid death rules — safe zone vs expedition spill (#27)
- Recoverable death caches — world marker + Settlement claim (#28)
- Currency — profile wallet (#29)
- Repeatable contracts — Settlement board (#30)
- Rewards and progression — credits + reputation on turn-in (#31)
- Complete expedition gameplay loop — session phases + F8 validator (#32)
