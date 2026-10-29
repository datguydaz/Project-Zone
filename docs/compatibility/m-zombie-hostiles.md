# Female Zombie hostiles (Outskirts)

**Pack:** `Assets/ZombieFemaleAnimations/` (FemaleZombie prefab + per-clip animator controllers)  
**Scene:** `PZ_Outskirts` via `OutskirtsNpcBootstrap`

## Approach

Full Emerald AI on this pack needs a authored Animation Profile (the pack ships one controller per clip). Until that profile exists, Outskirts spawns Female Zombie with Project Zone greybox melee:

| Piece | Role |
| --- | --- |
| `PzZombieHostileSetup` | Capsule + NavMeshAgent + HQ hit volume |
| `PzZombieHostileBrain` | Detect / chase / melee player |
| `PzHostileHealth` + `PzHostileDamageHitbox` | HQ weapon damage entry |
| `PzZombieAnimDriver` | Playables graph: Idle / Walk / Run / Attack / Death clips |

Clips are loaded from the Female Zombie FBX assets (not the pack’s one-clip-per-controller assets). Optional: **Project Zone → Build Female Zombie Animator** builds a multi-state controller for later Emerald wiring.

Checkpoint + Industrial spawn points use zombies. Optional Emerald ranged spawn remains behind `_spawnEmeraldRangedAtIndustrial` (off by default).

## Play Mode checks

1. Open `PZ_Outskirts`, enter Play Mode.
2. Approach Checkpoint (~45, 0, 4) — zombie should chase and melee.
3. Shoot zombie — health should drop; death plays Death01 controller and disables colliders.
4. Combat validator should count melee AI → player damage when hit by a zombie.

## Follow-up (Emerald)

Author an Emerald Animation Profile from the Female Zombie FBX clips, generate the Emerald Animator Controller, run Emerald Setup Manager on FemaleZombie, then swap bootstrap to that prefab for detection/behaviors while keeping `EmeraldDamageReceiver`.
