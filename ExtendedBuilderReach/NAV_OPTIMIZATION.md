# EBR demolition navigation optimization

## Motivation

When extended demolition is enabled, original EBR registers each target as a
navigation listener. Local terrain updates can require visiting many distant
targets. EBR now registers **one shared listener** and selects candidate targets
inside its own game-scoped service. No game navigation-registry patch is needed.

## Files and behavior

- \`Components/ExtendedDemolishableAccessible.cs\` still generates accesses,
  applies EBR reach settings, tests precise 3D bounds intersections, and
  disables the dedicated Accessible as before. It registers with the shared
  service and reports changes to its generated access bounds.
- \`Services/DemolitionNavigationService.cs\` implements \`INavMeshListener\`,
  \`IUpdatableSingleton\`, and \`IUnloadableSingleton\`. It indexes targets in
  8×8 XY grid buckets, uses \`NavMeshUpdate.Bounds\` to select candidates,
  and forwards to the unchanged EBR intersection logic.
- Updates selecting at least 1,024 candidates are deferred on the main thread:
  up to 256 candidates or 8 ms are processed per frame. All smaller updates
  are delivered in the original navigation callback. Deleted targets are
  ignored if their notification is still queued.
- Existing \`[HarmonyPatch]\` attribute-based EBR construction and demolition
  patches remain unchanged. The previous manual registry Harmony interception,
  emitted delegates, reflection, and manual frame-lifecycle patch are removed.

## Trade-offs and compatibility

Large navigation rebuilds can delay updates to demolition accessibility by
several frames. Callback order among EBR targets may differ, although exact
per-target bounds checks and reach logic remain unchanged. A wide bounding
rectangle containing sparse map changes can select extra candidates, which
the precise EBR check filters out.

This is game-scoped state, not static state. Unloading a game unregisters the
shared listener and clears pending batches. Do **not** run the separate
TB Optimizer EBR dispatcher at the same time as this integrated version.

## Validation

The independently enabled EBR module in TB Optimizer 1.1.120 has several
hours of user-reported normal gameplay in Timberborn 1.1.2.7 with EBR 11.0.0.
That supports the spatial-dispatch approach, but this **rewritten native EBR
service is a different implementation** and has not yet been game-tested.

- [ ] Compile with the maintainer's publicized Timberborn game assemblies.
- [ ] Test new/load saves and extended demolition on/off.
- [ ] Test terrain, roads, tall structures, and updated access bounds.
- [ ] Verify no stalled demolition jobs after large edits or target deletion.
- [ ] Compare frame time and achieved game speed at 1×/3× to original EBR.
