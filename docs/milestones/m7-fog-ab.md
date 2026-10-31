# M7 #54 — Fog A/B (Simple vs HAZE)

**Decision (2026-10-31):** **HAZE primary.** Developer Play Mode: ~270–300 FPS, look accepted (“amazing”).  
**Rule:** Never stack HAZE + Unity RenderSettings fog.

## Modes (Settings → Atmosphere → Fog Backend)

| Mode | What runs |
| --- | --- |
| **HazeVolumetric** (primary) | HAZE global fog on the Zone volume profile. `RenderSettings.fog = off`. |
| **SimpleLinear** (fallback) | `RenderSettings` linear fog from lighting profile. HAZE feature off. |

## Bench steps (developer)

1. Open `PZ_Outskirts`, Play Mode, Stats or FPS overlay on.
2. **A — Simple:** Fog Backend = `SimpleLinear`. Walk Settlement → Checkpoint → Industrial. Note avg FPS / 1% low (~30–60 s).
3. **B — HAZE:** Fog Backend = `HazeVolumetric` (auto-ensures `HazeRendererFeature` on `PC_Renderer`). Same path, same duration. Note FPS + visual (grit / sun shafts / height murk).
4. Record results in `docs/performance/BASELINE.md` or a comment on GitHub #54.
5. Choose primary: keep that Fog Backend as default; leave the other for optional quality toggle later if desired.

## Acceptance

- **Met (2026-10-31):** HAZE chosen — look + ~270–300 FPS Editor Play.
- One primary fog system for Outskirts: HAZE.
- Standalone re-measure still recommended later (Editor FPS is diagnostic).
