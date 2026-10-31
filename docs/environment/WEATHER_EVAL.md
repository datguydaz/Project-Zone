# Lightweight Weather Evaluation (#58)

**Status:** Prototype ready for FPS A/B — decision pending  
**Issue:** #58  
**Date:** 2026-10-31

## Question

Can Outskirts afford optional light rain/wind without breaking the 100 FPS floor (120–150+ typical)?

## What we already have (no new packs)

| System | Weather-adjacent role |
| --- | --- |
| HAZE + Zone Overcast | Sells “wet air” without rain |
| Ambient audio Outdoor bed | Wind hiss |
| TTFE foliage wind | Canopy motion |
| Dust particle hints | Light particulate |

## Prototype shipped

`PzWeatherDirector` — **camera-local** particle rain streaks only:

- Settings → Atmosphere → **Weather Mode** = `Off` / `LightRain`
- **Rain Intensity** 0–1
- Auto-stops when #57 space is **Indoor**
- No wetness, puddles, screen drops, audio bed swap, or decals

## A/B test (developer)

1. Play Mode Settlement gate, note FPS (rain Off)
2. Settings → Atmosphere → Weather Mode = **LightRain**, Save not required (live pull)
3. Same view, note FPS
4. Enter Hab (Indoor) — rain should stop

### Decision guide

| Result | Verdict |
| --- | --- |
| FPS drop ≤ ~15% and still ≥ ~200 Editor / comfortable feel | **Accept** as optional toggle (default Off) |
| Noticeable hitch / drop below comfort | **Defer** to post-M7 polish |
| Looks wrong / fights HAZE | **Reject** rain; keep HAZE + audio wind only |

## Preliminary recommendation (pre-metrics)

**Lean Defer for production default** — Zone Overcast + HAZE already sells weather mood. Keep LightRain as an **optional** Settings toggle after you record the A/B numbers; do not add wetness systems in M7.

## Non-goals (#58)

- Storms, lightning, snow
- Screen rain distortion
- Surface wetness / puddle shaders
- New Asset Store weather kits
