# DependencyInjection/Specs

## Purpose

Defines the interfaces and abstract base classes for modifying Timberborn's component-spec blueprints after `SpecService.Load`, plus the two services that coordinate the execution of those modifications.

## Key types

- **`ISpecServiceFrontRunner`** — Marker interface extending `ILoadableSingleton`. Services bound to this interface have their `Load()` called *before* `SpecService.Load`. No extra methods required.
- **`ISpecServiceTailRunner`** — Interface with a single `Run(SpecService)` method called *after* `SpecService.Load`.
- **`ISpecModifier`** — Core modification interface. Provides `Type` (the `ComponentSpec` type to target), `Order` (execution priority), `ShouldRun` (runtime gate), and `Modify(IEnumerable<EditableBlueprint>) -> IEnumerable<EditableBlueprint>`.
- **`BaseBlueprintModifier<T>`** — Abstract base implementing `ISpecModifier`; sets `Type` from the generic parameter and leaves `Modify` abstract.
- **`NamedSpec<T>`** — Readonly record struct pairing a blueprint name with a single typed spec, used by `BaseSpecModifier<T>`.
- **`BaseSpecModifier<T>`** — Extends `BaseBlueprintModifier<T>`; unpacks blueprints into `NamedSpec<T>` records and reconstructs them after modification. Use when each blueprint has exactly one spec of type `T` and no children need to be preserved.
- **`BaseSpecTransformer<T>`** — Extends `BaseBlueprintModifier<T>`; iterates blueprints and calls `Transform(T) -> T?` on each matching spec in place. Simpler than `BaseSpecModifier` when structural changes to the blueprint are not needed.
- **`SpecModifierService`** — `ISpecServiceTailRunner` that groups all multi-bound `ISpecModifier`s by `Type` into a `FrozenDictionary`, then applies them in `Order` sequence to the relevant blueprints in `SpecService._cachedBlueprintsBySpecs`.
- **`SpecServiceRunner`** — `IBlueprintModifierProvider` and `ILoadableSingleton` coordinator. Injected with both front-runner and tail-runner collections; fires all tail runners after `SpecService.Load` via `OnSpecLoaded`.

## How it fits together

Mod authors create a class inheriting `BaseSpecModifier<T>` or `BaseSpecTransformer<T>` (or implementing `ISpecModifier` directly) and register it with `configurator.BindSpecModifier<T>()`. `SpecModifierService` aggregates all such modifiers and is itself registered as an `ISpecServiceTailRunner` via `BindSpecTailRunner`. `SpecServiceRunner` collects all tail runners and is triggered by the Harmony postfix in `Patches/SpecServicePatches.cs`.

For more control over the full `SpecService` state (not just per-type blueprint batches), a mod can implement `ISpecServiceTailRunner` directly and register with `BindSpecTailRunner`.

## Dependencies & patterns

- **Bindito multi-bind**: `ISpecModifier` and `ISpecServiceTailRunner` are both collected as `IEnumerable<T>` in their respective consumers.
- **`FrozenDictionary` / `ImmutableArray`**: Used in `SpecModifierService` for efficient dispatch after construction.
- **Timberborn internals**: `SpecModifierService.Run` accesses `SpecService._cachedBlueprintsBySpecs` via reflection to read the cached blueprint map.

## Notes / gotchas

- `SpecServiceRunner.Load()` is intentionally empty; the `ILoadableSingleton` implementation is only there to ensure the front-runners' `Load()` calls are sequenced correctly by the DI framework.
- The `frontRunners` constructor parameter of `SpecServiceRunner` is suppressed with `#pragma warning disable CS9113` — it is injected solely for side-effect (forcing front-runner construction), not used directly.
