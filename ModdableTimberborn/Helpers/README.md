# Helpers

## Purpose

General-purpose utilities and extension methods used throughout ModdableTimberborn. This folder provides shared enums, extension method groups, a safe-mutation collection, inventory discovery logic, and glue for Harmony patching — avoiding duplication across the feature-specific subsystems.

## Key types

- **`CommonExtensions`** (`partial` class, split across `CommonExtensions.cs`, `CharacterExtensions.cs`, `ConfigurationExtensions.cs`) — the central extension-method hub. Covers numeric helpers (`PercentOf`, `Percent`), localization (`THours`), `NumericComparisonMode` → char conversion, `FrozenDictionary` group-by, reflection-based `SetGetOnlyProperty`, `GameObject.RemoveComponent<T>`, `BaseComponent.GetComponentOrNull<T>`, `BonusType` → `BonusSpec` conversion, `GameModeSpec` → `GameDifficultyEnum`, and `ITimeTriggerFactory.CreateAndStart`.
- **`ConfigurationExtensions`** (partial of `CommonExtensions`) — extension methods on Bindito's `ConfigurationContext` and `Configurator`. Adds context predicate helpers (`IsGameContext`, `IsGameplayContext`, etc.), a `ToBindAttributeContext` mapper, and typed multi-bind shortcuts for `ITemplateCollectionServiceTailRunner`, `ITemplateModifier`, `ISpecServiceTailRunner`, and `ISpecModifier`.
- **`CharacterExtensions`** (partial of `CommonExtensions`) — extension methods on `CharacterType` and `BaseComponent`. Includes `IsBot`, `IsBeaver`, `IsWorker`, `GetCharacterType<T>`, `IsBuilder`, `IsBuilderWorkplace`, and accessors for `BonusManager` / `BonusTrackerComponent` / `PersistentBonusTrackerComponent`.
- **`AutomationExtensions`** — extension methods on `AutomatorConnection`. Three `TryConnecting` overloads that accept a `Guid?`, an `EntityComponent?`, or an `Automator?`, resolving them through `EntityRegistry` where needed.
- **`EntityExtensions`** — extension methods on `BaseComponent` and `EntityRegistry`. Provides `GetEntityId`, `GetUpdatableStatComponent`, `TryGetEntity`, `DescribeEntity`, and `TryGetAutomator`.
- **`InventoryFinder`** — static utility for locating an inventory on a `BaseComponent`. Probes a known priority list of inventory-bearing component types (`SimpleOutputInventory`, `Stockpile`, `Manufactory`, `GoodConsumingBuilding`, `WonderInventory`, `BreedingPod`, `FireworkLauncher`, `DistrictCrossingInventory`, `RecoveredGoodStack`) and returns an `InventoryInfo` value struct with the component, `Inventory`, and `InventoryType` flags.
- **`InventoryInfo`** — `readonly record struct(BaseComponent, Inventory, InventoryType)` returned by `InventoryFinder`.
- **`InventoryType`** — `[Flags]` enum: `In`, `Out`, `Unlimited`, `Both`.
- **`CommonEnums`** — shared enums not tied to a single subsystem: `BonusType` (six bonus categories), `ModifierPriority` (`Force`/`Additive`/`Multiplicative` as int ranks), `CharacterType` (`[Flags]`: `Bot`, `AdultBeaver`, `ChildBeaver`, plus composite values `Beavers`, `Workers`, `All`).
- **`DeferredCollection<TCollection, T>`** — mutation-safe wrapper around any `ICollection<T>`. Queues `Add`, `Remove`, and `Clear` operations issued during enumeration and flushes them when the last `SafeEnumerate` iterator exits. Tracks nesting depth so reentrant enumeration is handled correctly.
- **`DeferredList<T>` / `DeferredHashSet<T>`** — concrete shorthands over `DeferredCollection` for `List<T>` and `HashSet<T>`.
- **`PatchExtensions`** — helpers for writing Harmony patches. `PatchAwakePostfix` and `PatchStartPostfix` wire the patched component to its `BaseModdableComponent<TComp>` sibling. `TranspileAndThrowIfNotFound` guards transpiler lambdas: if the target instruction is never matched it throws with a full IL dump rather than silently patching nothing.
- **`ModdableTimberbornUtils`** — miscellaneous statics. Holds `AllCharacterTypes` array, the mutable `CurrentContext` property (set by configurators to track which Bindito context is active), a `LogVerbose` wrapper prefixing `[ModdableTimberborn]`, and `AddSlotsToPrefab` / `KeepAddingUntil` for blueprint slot manipulation.

## How it fits together

`CommonExtensions` is the largest file and is deliberately `partial` — the character, configuration, and core portions live in separate files but compile to one class. Feature code across ModdableTimberborn imports this namespace and calls the extension methods without any DI injection.

`InventoryFinder` is used by automation and building-interaction subsystems to discover what kind of inventory a building owns without knowing its concrete type upfront. The priority order in `LookForInventories` is significant: `ConstructionSite` is excluded from the general search and must be queried separately via `LookForConstructionSiteInventory`.

`DeferredCollection` is used wherever a collection can be modified by callbacks fired during iteration (e.g., event listener lists, tick subscriber lists). Consumers call `SafeEnumerate()` rather than iterating `Collection` directly.

`PatchExtensions` is called from Harmony postfix and transpiler patch methods. `PatchAwakePostfix` / `PatchStartPostfix` replace boilerplate that would otherwise appear in every Harmony patch class that delegates to a `BaseModdableComponent`.

`ModdableTimberbornUtils.CurrentContext` is written by the configurator infrastructure so that utilities can branch on the active Bindito scene at configuration time.

## Dependencies & patterns

- **Bindito (DI):** `ConfigurationExtensions` wraps `Configurator` multi-bind helpers and `BindAttributes`. `ModdableTimberbornUtils.CurrentContext` tracks the active `ConfigurationContext`.
- **Harmony:** `PatchExtensions` uses `CodeInstruction`, `StrongBox<bool>`, and IL opcode/operand inspection — a direct Harmony/Cecil dependency.
- **Timberborn game types:** Heavy use of `BaseComponent`, `EntityRegistry`, `Automator`, inventory types, `BonusManager`, `Worker`, `Workplace`, `Blueprint`, spec types, and `ILoc` localization — all Timberborn game assembly references.
- **Unity:** `GameObject`, `Object.Destroy`/`DestroyImmediate`, and `Debug.LogWarning` appear in `CommonExtensions` and `ModdableTimberbornUtils`.
- **C# extension syntax (C# 14 `extension` blocks):** The codebase uses the new `extension(Type t) { ... }` syntax preview feature rather than classic `this`-parameter extension methods. This is visible in `AutomationExtensions`, `CommonExtensions`, `EntityExtensions`, etc.
- **Immutable collections:** `ImmutableArray`, `FrozenDictionary` used in extension helpers and `ModdableTimberbornUtils`.
- No DI registrations live in this folder — these are pure utilities consumed by higher-level configurators elsewhere.

## Notes / gotchas

- **`ModdableTimberbornUtils.CurrentContext` is `internal set`:** It is written by the configurator layer and read by helpers. Bindito configuration runs single-threaded, so this is safe in normal usage.
- **`CharacterType.ArrayLength`** is defined as `HighestBit + 1 = 5`, which equals the numeric value of `ChildBeaver` (4) plus 1. This is intended for array indexing by flag value but is only meaningful if flag values stay contiguous powers of two.
- The `extension(...)` syntax is C# 14 preview; this requires an appropriate `<LangVersion>` in the project file and may not be widely familiar to contributors.
