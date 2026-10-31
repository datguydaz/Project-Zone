# Indoor / Outdoor Atmosphere Transitions (#57)

**Status:** Play confirmed 2026-10-31 — #57 closed  
**Issue:** #57  
**Depends on:** #54 HAZE, #56 audio (independent volumes)

## Spaces

| Space | Where | Look |
| --- | --- | --- |
| **Outdoor** | Outside Settlement | Full HAZE / sun / ambient from Atmosphere Settings |
| **Sheltered** | Settlement courtyard | Less haze, slightly dimmer sun |
| **Indoor** | Hab / warehouse / tunnel | Much clearer air, lower sun, exposure bias up |

Audio (#56) stays Outdoor ↔ Settlement. Lighting spaces are separate volumes.

## Setup

1. Settings → **Atmosphere** → **Seed Space Zones**
2. Save scene
3. Play Mode: open road → Settlement gate → Hab block  
   Console: `Space → Outdoor` / `Sheltered` / `Indoor`  
   HAZE should thin indoors.

## Tuning

Settings → Atmosphere → **Space transitions** (multipliers + blend seconds).
