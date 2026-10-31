# Lighting, Terrain & Weather Strategy (M7)

**Status:** Active — M7 started 2026-10-30  
**Issue:** #51 unified environmental lighting strategy  
**Order:** Atmosphere (M7) before dense Outskirts environment art dress

## Principles

- **One lighting approach** for Outskirts (URP + profile presets).
- **One fog system:** HAZE volumetric (primary, #54). Never stack with RenderSettings fog. SimpleLinear kept as fallback.
- **One foliage workflow** (#55 locked): Toby Rocky Hills / Juniper + TTFE + PZ Prefab Paint. No Terrain-detail dual stack. See `docs/environment/FOLIAGE_WORKFLOW.md`.
- Prefer limited realtime shadow casters; soft shadows on the single directional sun.
- Weather only if cost is proven (#58).

## Target mood (Zone / Stalker)

**Primary day ref:** Stalzone / Stalcraft overcast gameplay — flat grey sky, soft sun disc in cloud, muddy desaturated midtones, dim but **fully readable** asphalt/concrete/foliage. Not pitch-black; not sunny FPS.

Also: dense air at distance, light grain, soft shadows. Autumn canopy OK — grade sells the Zone more than swapping trees.

**Art massing** (separate from lighting): blocky Soviet prefab geometry with photoreal materials — see `docs/environment/ZONE_VISUAL_STYLE.md`.

| Lever | Zone push | Combat guardrail |
| --- | --- | --- |
| Fog | Start ~8–15 m, end ~70–100 m | Enemies readable inside ~40 m |
| Saturation | −25 to −40 | Gun/UI still clear |
| Sun | Low angle, warm-sickly, intensity &lt; 0.6 | Soft shadows ok |
| Grain | ~0.15–0.22 | Don’t exceed ~0.3 |
| Chromatic | ~0.03 | Keep subtle |
| Sharpness | ~0.45 TAA CAS | Don’t crunch |
| Vignette | ~0.22 | Don’t hide HUD corners |

## Locked for #51 / #52

| System | Decision | Notes |
| --- | --- | --- |
| Pipeline | Unity 6 URP | Existing project stack |
| Presets | `PzLightingProfile` ScriptableObjects | `Resources/Atmosphere/` |
| Default look | **Zone Overcast** | Dense fog + desat + grain (Stalker lean) |
| Night | **Zone Night** stub | Finalize readability under #53 |
| Fog (primary) | HAZE volumetric | #54 chose HAZE (~270–300 FPS Editor Play); SimpleLinear = fallback; never stack |
| Volume | Global URP Volume | ColorAdjustments + SMH + FilmGrain + light Bloom + Vignette |
| Sun | Single Directional Light | Soft shadows, profile-driven |
| Skybox | `PZ_Sky_Overcast` / `PZ_Sky_Night` (procedural) | Replaces default blue sky — grit comes from grey overcast first |
| Scene View | `PzSceneViewGameLook` (default ON) | Fog + skybox + URP post + PostProcessing volume layer so Edit ≈ Game |

## Apply (Editor)

1. `Tools → PROJECT ZONE → Atmosphere → Ensure Lighting Profiles`
2. `Tools → PROJECT ZONE → Atmosphere → Apply Overcast Day (active scene)`
3. Save scene · Play Mode · check combat readability
4. Optional: Apply Night for a first pass (#53 still owns sign-off)

## Runtime

`OutskirtsAtmosphereBootstrap` loads profiles from Resources and applies the active preset on enable.

## Avoid

- Stacking HAZE + RenderSettings fog + vendor demo volumes.
- Multiple directional suns.
- Dropping vendor lighting demo settings into production scenes.
- Uncapped transparent foliage (World spacing caps + ~800 painted-prop soft cap).

## Follow-on M7 issues

| Issue | Focus |
| --- | --- |
| #52 | Overcast day Play Mode visual sign-off |
| #53 | Night preset combat readability |
| #54 | HAZE vs simple fog benchmark |
| #55 | Primary foliage workflow — **closed** (Toby + Paint; ~280 FPS Play) |
| #56 | Ambient audio zones — director + procedural beds (Play confirm) |
| #57 | Indoor/outdoor atmosphere transitions (lighting/fog; audio already #56) |
| #58 | Lightweight weather accept/defer |
| #59 | Environmental storytelling standards |
| #60 | Atmosphere checkpoint + profile |
