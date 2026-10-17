# BenchmarkAndOptimizerV11Port — tick safety rules

Purpose: restore original benchmarking / CSV profiling separately from gameplay optimizations.

## Hard-earned tick protections
- Never throttle `TickableBucketService.TickBuckets`: this is a **global** dispatcher (128 entity buckets + singleton bucket); throttling starved planting and unrelated systems.
- Always keep `PhysicsSimulator`, `BehaviorManager`, `NavMeshObserver`, `NavigationSynchronizer`, `AutomationRunner`, `PlantingControler`, `PlantMonitor`, `SoilContaminationService`, `WaterSimulator`, `SpeedManager`, `ConstructionSite`, `FramesPerSecondPanel`, `FramesPerSecondCounter` at their authoritative/vanilla cadence.
- `PlantingControler` and `PlantMonitor` were checked working; contamination-event planting stalls should not be diagnosed by slowing global ticks.
- When throttling concrete tick methods, preserve inherited-method sharing and resolve actual runtime concrete types.
- Do not patch hundreds of mod methods for diagnostics; use bounded allowlists and compile-check Harmony signatures.
- Keep benchmark mode explicitly opt-in and bounded (original 120s CSV workflow).
- Avoid scanning unfinished MonoMod/Harmony dynamic assemblies during Bindito / SpecTypeCache type discovery.

## Notes
These are safety policies, not a claim that every profiler or optimization has been restored to an independent build yet.
