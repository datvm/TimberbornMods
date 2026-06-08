# BuildingSettings

## Purpose

Defines a generic serialization/deserialization framework for copying building configuration between buildings (the "duplicate settings" feature). Each `IBuildingSettings` implementation knows how to snapshot one `IDuplicable` component type to JSON and reapply it to another instance of the same type. This lets the mod transfer virtually any building's state atomically and consistently.

## Key types

- **`IBuildingSettings`** — core interface: `Serialize`, `Deserialize`, `DescribeDuplicable`, `DescribeModel`, `CanDeserialize`, `GetComponent`. Has a typed generic variant `IBuildingSettings<T, TModel>` that provides covariant default bridge implementations.
- **`BuildingSettingsBase<T, TModel>`** — abstract base for most concrete handlers. Implements `ILoadableSingleton` to lazily resolve the localized `Name` key (`LV.MT.BldSet.<TypeName>`). Serializes via `JsonConvert`, applies via `ApplyModel`, and exposes a `DeserializeInternal` helper with optional model transformation.
- **`EntityIdBuildingSettingsBase<T, TModel>`** — extends `BuildingSettingsBase` for handlers whose model contains entity GUIDs. During deserialization it remaps those GUIDs through an `idMappings` dictionary (used when the source and target saves have different entity IDs — i.e., copy-paste across saves).
- **`IEntityIdBuildingSettings` / `IEntityIdBuildingSettingsBase<T, TModel>`** — interface pair marking handlers that need GUID remapping; callers use these to decide whether to pass an `idMappings` dictionary.
- **`IEntityIdModel` / `EntityIdModelBase`** — model base that exposes a `Guid?[]` slot array so `EntityIdBuildingSettingsBase` can do the GUID substitution loop generically.
- **`BuildingSettingsResolver`** — singleton that aggregates all `IBuildingSettings` instances injected via multibind, indexes them by `Type` into a `FrozenDictionary`, and exposes `Get(Type)`, `Get(IDuplicable)`, and `Get(BaseComponent[, filter])` helpers. Also caches the per-template component list in a `Dictionary<string, ImmutableArray<IBuildingSettings>>` to avoid repeated reflection.
- **`BuildingSettingsPair`** — lightweight `readonly record struct(IDuplicable, IBuildingSettings)` returned from resolver lookups.
- **`BuildingSettingsConfig`** — `IModdableTimberbornRegistryConfig` that registers `BuildingSettingsResolver` as a singleton and auto-discovers every concrete `IBuildingSettings` class in the assembly via reflection, binding each as a multibind singleton. Activated through `ModdableTimberbornRegistry.UseBuildingSettings()`.

## How it fits together

`BuildingSettingsConfig.Configure` scans the assembly for all non-abstract `IBuildingSettings` implementations and registers them as a multibound collection under `IBuildingSettings`. At game load, Bindito injects this collection into `BuildingSettingsResolver`, which builds the `FrozenDictionary<Type, IBuildingSettings>` index and the ordered `AllBuildingSettings` array.

The typical consumer (e.g., a UI panel or a blueprint copier) calls `BuildingSettingsResolver.Get(BaseComponent)` to get `BuildingSettingsPair[]` for a target building — one pair per duplicable component — then calls `pair.Settings.Serialize(pair.Duplicable)` to snapshot, and `pair.Settings.Deserialize(json, targetDuplicable)` to reapply. For entity-linked settings the caller checks `IEntityIdBuildingSettings` and passes the GUID map.

The concrete handlers live in `BuiltInSettings/` and cover all stock Timberborn `IDuplicable` component types.

## Dependencies & patterns

- **Bindito (DI):** `[MultiBind]` pattern via `configurator.MultiBind(typeof(IBuildingSettings), t).AsSingleton()` — no attribute, done reflectively in config.
- **Newtonsoft.Json:** `JsonConvert.SerializeObject` / `DeserializeObject` used directly in `BuildingSettingsBase`; some models carry `[JsonIgnore]` and `[JsonConstructor]` attributes.
- **`ILoadableSingleton`:** Used by `BuildingSettingsBase` (to resolve localized name on load) and `BuildingSettingsResolver` (to run optional dev-time validation).
- **`ILoc`:** Timberborn localization service, injected into every handler and base class.
- **No Harmony patches** in this folder.

## Notes / gotchas

- `BuildingSettingsResolver.ValidateBuildingSettingsHandler()` is commented out (`// Only run while developing`) — it would log warnings for any `IDuplicable` component type lacking a handler. Uncomment for coverage audits.
- The `Order` property on `IBuildingSettings` defaults to `1000`; `PausableBuildingSettings` overrides it to `100` so pause state is applied before other settings. Ordering matters if downstream consumers apply settings in sequence.
- Assembly auto-discovery in `BuildingSettingsConfig` means any new concrete `IBuildingSettings` added to the assembly is registered automatically — no manual wiring needed.
