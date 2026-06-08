# MechanicalSystem

## Purpose

Wraps Timberborn's `MechanicalNode` component in ModdableTimberborn's modifier pipeline so that a node's nominal power input and output can be altered at runtime by registered modifiers. Without this subsystem, a building's power values are fixed at prefab-load time and cannot be changed by mods.

## Key Types

- **`ModdableMechanicalNode`** — Decorator component (`BaseModdableComponent<MechanicalNode>`) attached alongside every `MechanicalNode`. Owns a `MechanicalNodeModifierCollection`, reacts to dirty notifications, and pushes recalculated values back into the wrapped node via `UpdatePowerInput()` / `UpdatePowerOutput()`. Raises `OnMechanicalNodeValuesChanged` after each recalculation.
- **`MechanicalNodeValues`** — Immutable `readonly record struct` holding the two values that can be modified: `NominalInput` and `NominalOutput`.
- **`ModdableMechanicalNodeValues`** — Thin `ModdableValue<MechanicalNodeValues>` wrapper; passed into each modifier in the chain and carries the current (mutable) and original values.
- **`IModdableMechanicalNodeModifier`** — Marker interface extending `IModdableModifier<ModdableMechanicalNodeValues, MechanicalNodeValues>`. Mod code implements this to supply power modifiers.
- **`ModdableMechanicalSystemConfig`** — `IModdableTimberbornRegistryWithPatchConfig` singleton that registers the Harmony patch category and binds `ModdableMechanicalNode` as a template decorator for `MechanicalNode` via Bindito's `BindTemplateModule`.

## How It Fits Together

At game startup, `ModdableTimberbornRegistry.UseMechanicalSystem()` (a partial method on the shared registry) registers `ModdableMechanicalSystemConfig.Instance` as a configurator. The configurator adds `ModdableMechanicalNode` as a decorator on every `MechanicalNode` prefab in the game context.

`MechanicalNodePatches` hooks `MechanicalNode.Awake` with a postfix that calls `PatchAwakePostfix<MechanicalNode, ModdableMechanicalNode>()` — a framework helper that sets `OriginalComponent` on the decorator and then calls `AwakeAfter()`. Inside `AwakeAfter`, the decorator reads the node's current `_nominalPowerInput`/`_nominalPowerOutput` to seed `MechanicalNodeValues`, and wires up a `MechanicalNodeModifierCollection` whose `OnDirty` triggers `UpdateValues()`.

When any registered `IModdableMechanicalNodeModifier` fires `OnChanged`, the collection marks itself dirty. The next call to `UpdateValues()` runs the full modifier chain, compares old/new values, and (if changed) writes back into the original component and fires `OnMechanicalNodeValuesChanged` for any downstream listeners.

Consumers (mod features) add modifier instances to the component's collection and listen to `OnMechanicalNodeValuesChanged` for UI or logic updates.

## Dependencies & Patterns

- **Harmony** — Two patch classes under `Patches/`, grouped under the category `ModdableTimberborn.MechanicalSystem`. Patches are applied/removed as a unit via that category name.
- **Bindito (DI)** — `BindTemplateModule` + `AddDecorator` attaches `ModdableMechanicalNode` to Timberborn's prefab template system; no `[MultiBind]` attributes are used here directly.
- **Common framework** — Inherits `BaseModdableComponent<T>`, `ModdableValue<T>`, `IModdableModifier<,>`, and the implicit `ModifierCollection` / `ValueModifierCollection` infrastructure from `ModdableTimberborn.Common`.
- **Registry pattern** — `ModdableTimberbornRegistry.UseMechanicalSystem()` is a fluent opt-in method (idempotent guard via `MechanicalSystemUsed`), consistent with all other subsystems in this library.

## Notes / Gotchas

- `MechanicalGraphPatches.PowerIsSuppliedWhenZeroConsumption` fixes a vanilla edge case: a `MechanicalGraph` with zero demand is not considered `Powered` by default. This patch forces `Powered = true` when `PowerDemand == 0`, which is a behaviour change affecting all mechanical networks (not just moddable ones) once the category is enabled.
- `MechanicalNodePatches.PatchWhenZeroUsage` patches `MechanicalNode.CanPotentiallyBePowered` to return `true` for nodes with zero nominal input, bypassing the vanilla `NoPowerStatus` check. Both patches alter base game behaviour and may interact with other mods that touch power logic.
- There is no persistence (save/load) of modified values; modifiers are expected to re-apply themselves on load.
