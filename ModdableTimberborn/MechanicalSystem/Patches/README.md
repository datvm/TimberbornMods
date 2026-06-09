# MechanicalSystem/Patches

## Purpose

Contains all Harmony patches for the `MechanicalSystem` subsystem. These patches hook into vanilla `MechanicalNode` and `MechanicalGraph` to inject the modding layer and fix edge-case vanilla bugs exposed when power values become dynamic.

## Key Types

- **`MechanicalNodePatches`** — Static patch class targeting `MechanicalNode`. Hooks `Awake` (postfix) to initialise the `ModdableMechanicalNode` decorator, and patches `CanPotentiallyBePowered` (prefix) to return `true` for nodes whose nominal input is zero or below.
- **`MechanicalGraphPatches`** — Patch class targeting `MechanicalGraph`. Adds a postfix to the `Powered` getter so that a graph with zero `PowerDemand` reports itself as powered (vanilla returns `false` in this case).

## How It Fits Together

Both classes share the Harmony category `ModdableTimberborn.MechanicalSystem` (from `ModdableMechanicalSystemConfig.PatchCategoryName`). They are activated as a unit when `UseMechanicalSystem()` is called on the registry, and can be deactivated together by un-applying that category.

`MechanicalNodePatches.AwakePostfix` delegates to the framework helper `PatchAwakePostfix<MechanicalNode, ModdableMechanicalNode>()`, which locates the decorator on the same GameObject, sets `OriginalComponent`, and calls `AwakeAfter()` — bootstrapping the modifier pipeline for that node.

## Dependencies & Patterns

- **Harmony** — `[HarmonyPatchCategory]`, `[HarmonyPatch]`, `[HarmonyPostfix]`, `[HarmonyPrefix]` attributes; standard Harmony 2.x conventions.
- No DI or Bindito involvement; patches operate directly on vanilla component instances.

## Notes / Gotchas

- The zero-demand `Powered` fix in `MechanicalGraphPatches` is a global behaviour change for all mechanical graphs, not scoped to moddable nodes. This could affect vanilla UI (e.g., power-supply indicators on empty networks).
