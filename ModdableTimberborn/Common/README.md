# Common

## Purpose

Shared abstractions and base types used across the ModdableTimberborn library. Defines the core modifier/value pipeline (how arbitrary numeric or typed values get transformed by a priority-ordered stack of modifiers) and the togglable-container pattern (how a component that owns a set of child components can be activated/deactivated while keeping event wiring consistent).

## Key types

- **`IModdableModifier`** — Base modifier interface: `Id`, `Priority`, `Disabled`, and `OnChanged` event. All modifiers implement this.
- **`IModdableModifier<TModdableValue, TValue>`** — Typed extension; adds `Modify(TModdableValue)` which mutates the value in-place and returns `true` to short-circuit the chain.
- **`IModdableValue<TValue>` / `ModdableValue<TValue>`** — Holds both the live `Value` and the `OriginalValue` that was set before modifiers ran. `ModdableValue<T>` is the concrete implementation; supports deconstruction.
- **`ModdableValueChanged<TValue>`** — Immutable `readonly record struct` carrying `(NewValue, OldValue)` for change-event payloads.
- **`ModifierCollection<T>`** — Owns an `ImmutableArray<T>` of modifiers sorted by `Priority` at construction, subscribes to each modifier's `OnChanged` to mark itself dirty. Implements `IDisposable` to unsubscribe.
- **`ValueModifierCollection<T, TModdableValue, TValue>`** — Extends `ModifierCollection` with a `Modify(TModdableValue, forceDirty)` method: resets value to original, walks the sorted modifier list skipping disabled entries, stops early on a short-circuit, clears `IsDirty`, and returns whether the value actually changed.
- **`PriorityItem<T>`** — Generic `record` wrapping an item with a priority int; implements `IComparable<PriorityItem<T>>` with hash-code tiebreaking.
- **`IModifiableEvent<TModifier, TValue>`** — Interface representing an in-flight event that carries a modifier list, original value, and a mutable current value (used in event-based modifier patterns).
- **`ITogglableContainer<TContainer, TMember>`** — Interface for an activate/deactivate relationship between a container `BaseComponent` and its member `BaseComponent`s. Exposes `Toggle(bool)`, default `Activate()`/`Deactivate()` helpers, and a `Toggled` event.
- **`BaseTogglableContainer<TContainer, TMember>`** — Abstract non-Unity class implementing `ITogglableContainer`. Guards against redundant toggles; calls abstract `PerformActivate`/`PerformDeactivate` and fires the `Toggled` event.
- **`BaseEventTogglableContainer<TContainer, TMember>`** — Extends `BaseTogglableContainer`: activation calls `AddEventHandlers()` then `OnMemberAdded` for each existing member; deactivation calls `RemoveEventHandlers()` then `OnMemberRemoved`. Concrete subclasses supply those four abstract methods.
- **`BaseEventTogglableContainerComponent<TTogglable, TContainer, TMember>`** — Unity `BaseComponent` wrapper. `Awake` creates the inner `TTogglable` data object (via abstract `CreateData`) and forwards its `Toggled` event to its own.
- **`BaseModdableComponent<TComponent>`** — Thin `BaseComponent` subtype that exposes `OriginalComponent` — a reference to the vanilla Timberborn component being modded/extended.
- **`IModdableComponentAwake` / `IModdableComponentStart`** — Marker interfaces with `AwakeAfter()` / `StartAfter()` hooks, called after Unity's own `Awake`/`Start` lifecycle methods.

## How it fits together

The modifier pipeline flows like this: a concrete value holder wraps a `ModdableValue<T>`, a `ValueModifierCollection` is created from all `IModdableModifier` components on the same `BaseComponent`, and whenever the collection is dirty (any modifier fired `OnChanged`) a call to `Modify(value)` resets the value to its original and walks the sorted, non-disabled modifier list. Modifiers mutate `IModdableValue.Value` directly and may short-circuit. The return value signals whether anything changed, letting callers decide whether to broadcast downstream.

The togglable-container hierarchy separates data logic from Unity lifecycle: `BaseTogglableContainer` / `BaseEventTogglableContainer` are plain C# classes (no Unity dependency) and hold the activate/deactivate state machine. `BaseEventTogglableContainerComponent` bridges into Unity — it creates the data object in `Awake` and delegates to it. Feature-specific containers (elsewhere in the project) inherit from `BaseEventTogglableContainer` and supply the event-wiring and per-member callbacks.

`BaseModdableComponent<T>` and the `IModdableComponentAwake`/`IModdableComponentStart` interfaces are the entry point for the modding side: when ModdableTimberborn patches a vanilla component, it sets `OriginalComponent` on the modded wrapper and calls `AwakeAfter`/`StartAfter` to let the mod run post-vanilla-initialization logic.

## Dependencies & patterns

- **Timberborn / Unity:** `BaseComponent` is a Timberborn wrapper around `UnityEngine.MonoBehaviour`. `GetComponent`/`GetComponents` are Unity APIs used in `ModifierCollection` and `BaseEventTogglableContainerComponent.Awake`.
- **No DI registration here:** Common contains only abstract base types and interfaces; DI bindings (`[MultiBind]`, configurators) are handled by feature-specific modules that consume these types.
- **No Harmony patches** in this folder; patching is done elsewhere.
- **`ImmutableArray<T>`** (System.Collections.Immutable) used in `ModifierCollection` — modifier list is fixed at construction time; runtime changes require a new collection.

## Notes / gotchas

- `PriorityItem<T>` tiebreaks by `GetHashCode()` comparison, which is arbitrary and non-deterministic for types without a stable hash. Two items at the same priority with the same hash code compare as equal, which could cause unexpected behavior in sorted structures.
- `IModifiableEvent` is defined in this folder and consumed by event-bus-style modifier patterns implemented in feature-specific modules.
