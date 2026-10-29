# M4 — Inventory & Persistence

**Issues:** [#21](https://github.com/datguydaz/Project-Zone/issues/21)–[#26](https://github.com/datguydaz/Project-Zone/issues/26)  
**Primary scene:** `Assets/_ProjectZone/Scenes/PZ_Outskirts.unity`

## Intent

HQ FPS already owns backpack, holster, item defs, pickups, `ItemTransfering`, and scene save components. Milestone 4 wires that stack into PROJECT ZONE’s expedition loop — it does **not** replace HQ inventory.

## What shipped (adapters under `Assets/_ProjectZone/Runtime/Inventory/`)

| Issue | Adapter | Notes |
|---|---|---|
| #21 Loot containers | `PzWorldLootContainer` + `PzContainerHost` | Interactable cubes at Checkpoint / Industrial; HQ `ItemContainerGenerator` + `FPS_WeaponsAndAmmo` loot table |
| #22 Backpack UI | HQ Item Wheel (MMB) + loot/stash panel | Middle mouse = HQ Item Wheel. Loot/stash interact opens dual transfer panel over HQ backpack |
| #23 Permanent stash | `PzPermanentStash` + `PzStashInteractable` | Settlement SupplyShed terminal; profile file under `persistentDataPath/ProjectZone/` |
| #24 Transfers | Same panel | Click-select then click target; calls HQ `ItemTransfering` |
| #25 Save/load | `PzInventoryPersistenceValidator` + `SceneSaveHandler` | F5/F9 player snapshot; F6 stash; F7 HQ `LevelManager.SaveCurrentGame(0)` when game ID valid |
| #26 Persistent equipment | `PzPlayerInventorySnapshot` + HQ `Inventory` / `WieldableInventory` | Snapshot restores backpack + holster selection |

Bootstrap: `OutskirtsInventoryBootstrap` on `Outskirts Region` (spawns `Outskirts_Inventory` root outside greybox zones).  
Level definition: `Assets/_ProjectZone/Data/PZ_Outskirts_LevelDefinition.asset`.  
Editor menu: **Project Zone → Inventory → Setup Outskirts Inventory & Persistence**.

## Play Mode checklist (not run in this agent session)

1. Open `PZ_Outskirts`, enter Play Mode.
2. Middle mouse — HQ Item Wheel / inventory (existing HQ UI).
3. In Settlement **courtyard** (south of SupplyShed, ~`(5.5, 1, 5.5)`) — bright green **STASH** cube with beacon — interact; transfer; **F6**; restart Play Mode; confirm stash reload.
4. Bright **LOOT** beacons at Checkpoint / Industrial — interact, transfer into backpack.
5. **F5** snapshot player inventory; change gear; **F9** restore; confirm backpack/holster.
6. Optional: **F7** HQ scene save if Console shows CurrentGameID valid.

**Status:** Play Mode confirmed 2026-10-29 — issues #21–#26 and Milestone 4 closed.

### Starting loadout
`PzPlayerStartingLoadout` grants HQFPS_M1911 (same item as `HQFPS_Pickup_M1911`) into Holster plus .45 ACP in Backpack on Outskirts spawn.
