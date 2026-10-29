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
- Melee and ranged combat validation — ranged verified in play mode (#11); melee deferred until a melee hostile is in the test scene
- Death and hit reaction testing — verified in play mode (#12)
- Package compatibility validation — verified in play mode (#13)

## Milestone 3 — The Outskirts

- **Status:** Completed (awaiting final Play Mode confirm on #19/#20)
- **Scene:** `Assets/_ProjectZone/Scenes/PZ_Outskirts.unity` (keep `PZ_DamageAdapterTest` as M2 combat sandbox; leave `HQFPS_FiringRange` as vendor demo)
- First playable region layout — verified in play mode (#14)
- Safe settlement — verified greybox hub in play mode (#15); art/prop pass later
- Abandoned checkpoint — verified greybox in play mode (#16); art/prop pass later
- Industrial building — verified greybox in play mode (#17); art/prop pass later
- Underground entrance — verified greybox in play mode (#18); real transition/art later
- NPC placement and navigation — NavMesh bake + settlement civilian placeholder + Emerald hostiles at Checkpoint/Industrial (#19)
- Environmental atmosphere — dusk lighting, fog, URP volume, dust hints, ambient audio hook (#20)

## Milestone 4 — Inventory & Persistence

- **Status:** Planned (Not Started)
- Loot containers
- Backpack UI
- Permanent stash
- Inventory transfers
- Save/load validation
- Persistent equipment and items

## Milestone 5 — Death & Contracts

- **Status:** Planned (Not Started)
- Hybrid death rules
- Recoverable death caches
- Currency
- Repeatable contracts
- Rewards and progression
- Complete expedition gameplay loop
