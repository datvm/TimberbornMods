# Extended Builder Reach: spatial navigation dispatch

## Why

Extended demolition range registers a navigation listener for each demolition target.
On a large map, vanilla navigation notifications inspect all those listeners, even
when only a small area changed. This adds a spatial index for **EBR listeners only**.

## Implementation

- `Optimizations/ExtendedBuilderReachNavOptimizer.cs` intercepts EBR listener
  registration and uses 8×8 XY buckets to select affected listeners. It keeps
  the original 3D intersection check and `OnNavMeshUpdated` callback.
- The existing component reports changed access bounds so bucket membership
  remains current. Pre-existing listeners are migrated on the first notification.
- For broad changes (at least 1,024 candidates), the dispatcher snapshots
  candidates using a pooled array and drains up to 256 listeners or 8 ms per
  frame via `EbrDeferredDrainHook`. This intentionally changes callback timing.
- Other mods' and vanilla navigation listeners continue through the game's
  ordinary registry. This does **not** change construction reach, demolition
  rules, tick cadence, save handling, or `Accessible.SetAccesses`.

## Compatibility and trade-offs

- Runs on the Unity main thread; no concurrent navigation or Unity calls.
- If required game members or the frame hook cannot be resolved, the optimizer
  does not install. EBR's original navigation behavior stays in place.
- This uses game-version-specific navigation internals and needs retesting
  after Timberborn updates.
- Do **not** run it alongside the standalone TB Optimizer EBR dispatcher.
- Large navigation rebuilds can leave EBR callbacks pending into later frames.
  Exact same-frame ordering with vanilla is **not** promised.
- Instrumentation is opt-in at build time (`EBR_NAV_DIAGNOSTICS`).

## Field-testing status (October 2026)

The EBR dispatcher running **inside TB Optimizer 1.1.120** was exercised
for several hours in a heavily modded Timberborn 1.1.2.7 colony alongside
Extended Builder Reach 11.0.0. The player reported normal gameplay; the
provided Player.log contains no managed exceptions or EBR-specific failures
during that session. It does contain unrelated mod and localization warnings.

This is **integration evidence**, not an in-game test of this standalone
upstream build: TB Optimizer patches EBR's `UpdateAccesses` via Harmony;
this proposal calls `BoundsChanged` directly in EBR's component instead.
The runtime report does not independently verify callback completion,
measured FPS improvement, unchanged job ordering, or compatibility without
TB Optimizer. Those checks remain open below.

## Before merging

- [ ] Build `ExtendedBuilderReach` with the author's publicized game assemblies.
- [ ] Load new and existing saves with extended demolition enabled and disabled.
- [ ] Check construction and demolition boundaries at different heights.
- [ ] Test isolated terrain/road edits, large demolitions and full navmesh rebuilds.
- [ ] Remove and rebuild listeners while a batch is pending; check for job stalls.
- [ ] Compare frame times **and achieved simulation speed** at 1× and 3×
      on a repeatable large colony.
- [ ] Test alongside other navigation mods without another EBR dispatcher.

This is a proposed upstream extraction from the TB Optimizer experiment,
not a claim that the port was built or independently verified in-game.
