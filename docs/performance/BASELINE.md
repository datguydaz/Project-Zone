# PROJECT ZONE — Performance Baseline

**Status:** Procedure defined under M5.5. **No authoritative FPS numbers claimed yet.**  
Editor Play Mode is diagnostic only. Standalone Development builds are required for acceptance.

## Targets

| Context | Target |
| --- | --- |
| Typical gameplay | 120–150+ FPS |
| Stretch | 150+ FPS |
| Demanding combat/environments | ≥ 100 FPS |
| Release acceptance floor (reference config) | 100 FPS |
| Frame time (100 / 120 / 150 FPS) | 10 / 8.33 / 6.67 ms |

Prioritize stable frame times and 1% lows over peak averages.

## Reference configuration (to be filled by developer)

| Field | Value |
| --- | --- |
| CPU | _TBD — document on first standalone run_ |
| GPU | _TBD_ |
| RAM | _TBD_ |
| OS | Windows 10/11 |
| Resolution | _TBD (native; no DLSS/FSR to meet targets)_ |
| Quality preset | _TBD_ |
| VSync | Off for benchmarks |
| Build type | Development (profiling) / Release (spot check) |

## Metrics to record

- Average FPS, 1% low FPS
- CPU / GPU / main-thread frame time
- Memory (Reserved / Allocated)
- GC spikes
- Loading hitch duration
- Active Emerald AI count
- Visible renderers / draw calls / batches (Frame Debugger / Profiler)

## Procedure

1. Capture baseline on current `PZ_Outskirts` greybox (settlement idle + short traversal).
2. Introduce one system or art density change.
3. Re-measure same path/camera route.
4. Document benefit vs cost in `docs/performance/BENCHMARKS.md`.
5. Accept, optimize, defer, or reject.

Tools: Unity Profiler, Profile Analyzer (`com.unity.performance.profile-analyzer` present), Memory Profiler optional, Frame Debugger.

## Representative scenarios (future gates)

1. Settlement with NPCs  
2. Open field / forest sightlines  
3. Industrial combat  
4. Underground + dynamic lights  
5. Fog/weather on  
6. Multiple active Emerald AI  
7. Region transition  
8. Inventory open + save/load  

## Current measurements

| Date | Build | Scenario | Avg FPS | 1% low | Notes |
| --- | --- | --- | --- | --- | --- |
| — | — | — | — | — | Not measured in this pass |
