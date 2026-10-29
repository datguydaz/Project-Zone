# M5 — Death & Contracts

**Issues:** [#27](https://github.com/datguydaz/Project-Zone/issues/27)–[#32](https://github.com/datguydaz/Project-Zone/issues/32)  
**Primary scene:** `Assets/_ProjectZone/Scenes/PZ_Outskirts.unity`

## Intent

HQ provides death/respawn (`HealthManager`, `DeathUI`) but **no** hybrid zone rules, death caches, wallet, or contracts. Milestone 5 adds thin Project Zone adapters under `Assets/_ProjectZone/Runtime/Expedition/`.

## What shipped

| Issue | Adapter | Notes |
|---|---|---|
| #27 Hybrid death | `PzSafeZoneTracker` + `PzHybridDeathController` | Inside `SafeZoneVolume` = soft death (no spill). Outside = spill backpack to death cache |
| #28 Death caches | `PzDeathCacheService` + interactables | Orange world marker at death + Settlement **Lost & Found** on WatchPost deck |
| #29 Currency | `PzWallet` | Profile file `pz_wallet.sav`; credits granted on contract turn-in |
| #30 Contracts | `PzContractBoardService` + board | Blue **JOBS** board in courtyard; default Scout + Recover Cache contracts |
| #31 Rewards | Same board | Turn-in pays credits + reputation |
| #32 Loop | `PzExpeditionLoop` + validator | Phase AtHub / InField / DeadPendingRecovery; **F8** status |

Bootstrap: `OutskirtsExpeditionBootstrap` on `Outskirts Region`.

## Play Mode checklist

1. Play `PZ_Outskirts` — spawn with M1911; Console shows expedition checklist pulses.
2. Blue **JOBS** board (courtyard north) — interact to accept **Outskirts Scout**.
3. Walk east to Checkpoint (~`45,0,0`) — objective completes; return to board — turn in for credits.
4. Leave Settlement (leave green safe volume) with items in backpack; die (fall / AI) — backpack spills to orange **CACHE**; respawn; reclaim at cache or WatchPost **Lost & Found**.
5. **F8** prints phase / credits / contract / deathCache.

Leave #27–#32 open until this checklist is confirmed in Play Mode.
