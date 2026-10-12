# Extended Builder Reach — navigation optimization (Timberborn 1.1)

## Background / measured hot path

This change upstreams the EBR-specific spatial listener dispatcher developed
and profiled in TB Optimizer / BenchmarkAndOptimizerV11Port. In colonies with
tens of thousands of EBR demolition listeners, broad navigation notifications
otherwise enumerate and test every listener, even for small local edits.

These are *candidate-selection* improvements, not a change to demolition or
construction distance rules. The existing EBR functionality, including the
dedicated disabled `ExtendedDemolishable` accessible, remains authoritative.

## What changes in code

| File | Responsibility |
| --- | --- |
| `Optimizations/ExtendedBuilderReachNavOptimizer.cs` | Intercept **only EBR listener registrations**, maintain 8×8 XY buckets, select likely listeners for terrain/road changes, preserve the exact 3D bounds intersection before calling the existing `OnNavMeshUpdated`. Rent candidate buffers from `ArrayPool<object>` for large updates. |
| `Optimizations/EbrDeferredDrainHook.cs` | Drain up to 256 candidate checks or 8 ms per main-thread Unity update via `SingletonLifecycleService.UpdateSingletons` postfix. If this hook cannot be installed, the optimization is not installed. |
| `Components/ExtendedDemolishableAccessible.cs` | Reindex registered listeners when their regenerated access bounds actually change. Prevents stale XY buckets from missing navigation notifications. |
| `MStarter.cs` | Wire the isolated EBR optimization without requiring TB Optimizer, benchmark sampling, or freeze-detector logging. |

The dispatcher also migrates EBR listeners registered *before* its patches
on the first relevant `NotifyAll`. Other game or mod listeners stay on the
vanilla registry. Global tick cadence, save operations, base-game pathfinding
and other mods' listeners are not throttled.

## Safety conditions and limitations

- All work stays on Unity's main thread; no worker-thread interaction with
  navigation, Unity objects, or `Accessible.SetAccesses`.
- Exact 3D intersection check and original EBR callback are retained.
- Unrecognized coordinate/update shapes fall back to a full EBR scan.
- Existing `Accessible.SetAccesses` is **never skipped**. Earlier profiling
  suggested substantial cost in that method, but its side effects were not
  verified; therefore the experimental no-op fast path is **not** included.
- Large batches are deferred to later frames. This intentionally trades some
  notification latency for smoother frame time. Do not assert identical
  intra-frame listener ordering compared with vanilla.
- The implementation guards against co-installation with the old
  `tharbad08.TBOptimizer` or `tharbad08.TBEBROptimizations` dispatcher; nevertheless
  do not install a second EBR optimization mod concurrently.
- If critical game types, properties, fields, or the drain hook cannot be
  resolved at startup, the EBR dispatcher is not installed. This is not a
  guarantee of compatibility with future game builds.
- Diagnostic calls are compiled out by default (`EBR_NAV_DIAGNOSTICS`).

## Validation checklist (manual in-game; not covered by compilation)

1. Start a v1.1 colony with Extend Demolition Range **off** and confirm
   normal construction and demolition.
2. Turn it on, vary above/below reach, and verify reachable vs unreachable
   targets at the boundary at speed ×1 and ×3.
3. Test small terrain edits, roads, large multi-tile demolition projects,
   removing trees, ruins, and building sites; verify jobs resume after edits.
4. Trigger large navmesh rebuilds (>1024 EBR candidates), let the simulation
   run, and ensure all deferred work drains and no demolition target stalls.
5. Test after save/reload and while removing/rebuilding objects. Confirm no
   errors from stale listeners or pooled candidate buffers.
6. Repeat with known navigation-heavy mods, *without* the separate TB EBR
   optimizer; compare FPS and game speed on the same save.
7. Profile EBR updates and report callback counts, max frame times, and
   queued-work behavior. Existing results indicate this is a useful hot path,
   but no independent in-game validation of this upstream port is claimed.

## Rollback

Revert this PR to restore vanilla EBR listener registration/dispatch; the
original EBR reach and demolition patches are otherwise unchanged.

## Origin

Adapted from `tharbad08/TimberbornMods`,
`BenchmarkAndOptimizerV11Port/Optimizations/Mods/ExtendedBuilderReach/`
on branch `TB-Optimizer`. The previous experimental optimizer also included
diagnostics and an IL-guarded `SetAccesses` no-op attempt; those are deliberately
excluded from this upstream-facing PR.
