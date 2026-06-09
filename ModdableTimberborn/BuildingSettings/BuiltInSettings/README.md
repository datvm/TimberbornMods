# BuiltInSettings

## Purpose

Contains one concrete `IBuildingSettings` handler for every stock Timberborn `IDuplicable` component type, plus the shared model record types they use. Each handler snapshots a single component to JSON and reapplies it to a target component, with guard logic to skip inapplicable targets. This is the exhaustive "built-in" coverage that ships with ModdableTimberborn.

## Key types

### Shared model primitives (`CommonSettingModels.cs`)

- **`ValueSettingModel<T>`** — single-value wrapper record with implicit conversion to `T`.
- **`BoolSettingModel`**, **`IntSettingModel`**, **`FloatSettingModel`**, **`StringSettingModel`** — typed specializations; `BoolSettingModel` knows how to call `ILoc.TYesNo`.
- **`CachableStringSettingModel<T>`** — `StringSettingModel` subclass that lazily resolves a display name and a typed object (e.g., `RecipeSpec`, `GoodSpec`, `PlantableSpec`, `GatherableSpec`) on first access; avoids repeated lookups. Cached fields are `[JsonIgnore]`.
- **`PrioritySettingModel`** — wraps `Priority` with `ILoc.T` formatting.

### Shared model primitives (`ComparisonSettingsModel.cs`)

- **`ComparisonSettingsModel`** — `(NumericComparisonMode Mode, float Threshold)` record used by all threshold-style sensors.

### Entity-id model (`EntityIdModelBase` — in parent namespace)

Several models inherit `EntityIdModelBase` so that entity-GUID remapping works generically. Examples: `AutomatableSettingsModel`, `RelaySettingsModel`, `MemorySettingsModel`, `TimerSettingsModel`.

### Handler classes (one per component type)

| Handler | Component | Model | Notes |
|---|---|---|---|
| `AutomatableSettings` | `Automatable` | `AutomatableSettingsModel` | Entity-id; resolves/sets automator via `EntityRegistry`. |
| `BuilderPrioritizableSettings` | `BuilderPrioritizable` | `PrioritySettingModel` | Guards on `Enabled` or `BlockObject.IsFinished`. |
| `ChronometerSettings` | `Chronometer` | `ChronometerSettingsModel` | Calls `UpdateOutputState()` after apply. |
| `ClutchSettings` | `Clutch` | `ClutchSettingsModel` | Calls `ApplyState()`. |
| `ContaminationSensorSettings` | `ContaminationSensor` | `ComparisonSettingsModel` | — |
| `CustomizableIlluminatorSettings` | `CustomizableIlluminator` | `CustomizableIlluminatorSettingsModel` | Accesses private `_customColor` field; serializes color as `SerializableFloats`. |
| `DecalSupplierSettings` | `DecalSupplier` | `DecalSupplierSettingsModel` | Guards on `Category` match. |
| `DemolishableSettings` | `Demolishable` | `BoolSettingModel` | Calls `Mark()`/`Unmark()`. |
| `DepthSensorSettings` | `DepthSensor` | `ComparisonSettingsModel` | Calls `InitializeSensorCoordinates()` before writing threshold. |
| `DistrictDefaultWorkerTypeSettings` | `DistrictDefaultWorkerType` | `StringSettingModel` | Uses `WorkerTypeHelper` for display. |
| `FarmHouseSettings` | `FarmHouse` | `BoolSettingModel` | `PlantingPrioritized`. |
| `FireworkLauncherSettings` | `FireworkLauncher` | `FireworkLauncherSettingsModel` | Falls back to first available firework if stored ID is missing. |
| `FixedStockpileInventorySetterSettings` | `FixedStockpileInventorySetter` | `FixedStockpileInventorySetterSettingsModel` | Guards on `WhitelistedGoodType` match; accesses private `_stockpile` / `_singleGoodAllower`. |
| `FlippableDecalSettings` | `FlippableDecal` | `BoolSettingModel` | — |
| `FloodgateSettings` | `Floodgate` | `FloodgateSettingsModel` | Clamps height; calls `SynchronizeAllNeighbors()`. |
| `FlowSensorSettings` | `FlowSensor` | `ComparisonSettingsModel` | — |
| `ForesterSettings` | `Forester` | `BoolSettingModel` | `ReplantDeadTrees`. |
| `GateSettings` | `Gate` | `ValueSettingModel<GateOpeningMode>` | Accesses private `_gateOpeningMode`. |
| `GatherablePrioritizerSettings` | `GatherablePrioritizer` | `CachableStringSettingModel<GatherableSpec>` | Uses `TemplateNameMapper` to resolve spec; guards on `SupportsGatherable`. |
| `HaulPrioritizableSettings` | `HaulPrioritizable` | `BoolSettingModel` | `Prioritized`. |
| `HttpAdapterSettings` | `HttpAdapter` | `HttpAdapterSettingsModel` | Serializes webhook URLs and method; `DescribeModel` returns `""`. |
| `IndicatorSettings` | `Indicator` | `IndicatorSettingsModel` | Pins, warnings, journal entries, color replication; `DescribeModel` returns `""`. |
| `LeverSettings` | `Lever` | `LeverSettingsModel` | Calls `UpdateOutputState()`. |
| `ManufactorySettings` | `Manufactory` | `CachableStringSettingModel<RecipeSpec>` | Guards: skips if recipe is unchanged or not in `ProductionRecipes`, or only one recipe exists. Accesses private `_recipeSpecs` dictionary. |
| `MemorySettings` | `Memory` | `MemorySettingsModel` | Entity-id (3 GUIDs); connects inputs via `TryConnecting`. |
| `PausableBuildingSettings` | `PausableBuilding` | `BoolSettingModel` | `Order = 100` (applied first, before other settings). |
| `PlantablePrioritizerSettings` | `PlantablePrioritizer` | `CachableStringSettingModel<PlantableSpec>` | Guards on `AllowedPlantables` membership. |
| `PopulationCounterSettings` | `PopulationCounter` | `PopulationCounterSettingsModel` | Extends `ComparisonSettingsModel`; calls `Sample()`. |
| `PowerMeterSettings` | `PowerMeter` | `PowerMeterSettingsModel` | Handles both int-threshold and percent-threshold modes. |
| `RelaySettings` | `Relay` | `RelaySettingsModel` | Entity-id (2 GUIDs); calls `Evaluate()`. |
| `ResourceCounterSettings` | `ResourceCounter` | `ResourceCounterSettingsModel` | Silently skips unknown good IDs; calls `InvokeGoodChangeEvent` and `Sample()`. |
| `ScienceCounterSettings` | `ScienceCounter` | `ComparisonSettingsModel` | Casts threshold to `int`. |
| `SingleGoodAllowerSettings` | `SingleGoodAllower` | `CachableStringSettingModel<GoodSpec>` | Guards on `inventory.Takes(id)`. |
| `SluiceSettings` | `Sluice` | `SluiceSettingsModel` | Model carries a `[JsonIgnore] SluiceState` built from record fields; applies via `SetStateAndSynchronize`. |
| `SpeakerSettings` | `Speaker` | `SpeakerSettingsModel` | Validates sound ID via `SpeakerSoundService`; calls `StopAndPlayIfContinuous()`. |
| `StockpilePrioritySettings` | `StockpilePriority` | `ValueSettingModel<StockpilePriorityState>` | Local `StockpilePriorityState` enum defined in the same file. |
| `TimedComponentActivatorSettings` | `TimedComponentActivator` | `TimedComponentActivatorSettingsModel` | Constructs a temp `TimedComponentActivator(null,null,null,null)` and calls `DuplicateFrom`; `DescribeModel` returns `""`. |
| `TimerSettings` | `Timer` | `TimerSettingsModel` | Entity-id (2 GUIDs); two `TimerInterval` sub-models; accesses private `_hours` on interval. |
| `UnstableCoreSettings` | `UnstableCore` | `IntSettingModel` | Explosion radius. |
| `ValveSettings` | `Valve` | `ValveSettingsModel` | Calls `SynchronizeNeighbors()`; `DescribeModel` returns `""`. |
| `WaterInputCoordinatesSettings` | `WaterInputCoordinates` | `WaterInputCoordinatesSettingsModel` | Clamps to `_waterInputSpec.MaxDepth`; calls `UpdateCoordinatesAndDepth()`. |
| `WaterMoverSettings` | `WaterMover` | `WaterMoverSettingsModel` | Implements `ILoadableSingleton` itself to cache good display names on load (separate from the base class `Load()`). |
| `WaterSourceRegulatorSettings` | `WaterSourceRegulator` | `WaterSourceRegulatorSettingsModel` | Accesses private `_regulatorState`. |
| `WaterSourceSettings` | `WaterSource` | `FloatSettingModel` | `SpecifiedStrength`. |
| `WeatherStationSettings` | `WeatherStation` | `WeatherStationSettingsModel` | Calls `UpdateOutputState()`. |
| `WorkplacePrioritySettings` | `WorkplacePriority` | `PrioritySettingModel` | — |
| `WorkplaceSettings` | `Workplace` | `IntSettingModel` | Caps `DesiredWorkers` at `MaxWorkers`. |
| `WorkplaceWorkerTypeSettings` | `WorkplaceWorkerType` | `StringSettingModel` | Guards on `IsWorkerTypeAllowed` and `IsWorkerTypeUnlocked`. |

## How it fits together

Every class here extends either `BuildingSettingsBase<T, TModel>` or `EntityIdBuildingSettingsBase<T, TModel>`. The two abstract methods are `GetModel` (snapshot the component to a record) and `ApplyModel` (push a deserialized record back to the component). Handlers that need to look up game services (recipes, goods, entity registry, etc.) receive them via constructor injection — the DI container wires them because `BuildingSettingsConfig` registers all of them as singletons.

`CachableStringSettingModel<T>` is the go-to pattern whenever the serialized value is a string ID (recipe ID, template name, good ID) and display needs a resolved name: the first `EnsureModelCache` / `EnsureCached` call populates `CachedValue` and `CachedDisplay`, subsequent calls are no-ops.

Entity-linked handlers (`AutomatableSettings`, `RelaySettings`, `MemorySettings`, `TimerSettings`) inherit `EntityIdBuildingSettingsBase` and expose the remapping overload for cross-save paste.

## Dependencies & patterns

- **Timberborn game components:** Direct access to internal/private fields (e.g., `_gateOpeningMode`, `_customColor`, `_sluiceState`, `_recipeSpecs`, `_hours`, `_rawThreshold`) via the modding assembly which exposes them.
- **`ILoc`:** All handlers accept it for localized descriptions and `TNone()` / `TYesNo()` helpers.
- **`EntityRegistry`:** Used by entity-id handlers to resolve `Automator` by GUID and to format `DescribeEntity`.
- **`IGoodService`, `RecipeSpecService`, `FireworkSpecService`, `SpeakerSoundService`, `TemplateNameMapper`, `WorkerTypeHelper`:** Injected where needed for lookups and display text.
- **No Harmony patches** in this subfolder.
- **`[JsonIgnore]`** on computed/cached properties in models; **`[JsonConstructor]`** on `CachableStringSettingModel<T>` to avoid ambiguity.

## Notes / gotchas

- `WaterMoverSettings` overrides `ILoadableSingleton.Load()` with `base.Load()` followed by its own logic; the explicit interface implementation is required because `BuildingSettingsBase` also implements `ILoadableSingleton`.
- `TimedComponentActivatorSettings` constructs `TimedComponentActivator(null, null, null, null)` to call `DuplicateFrom` — relies on the game component being null-safe in that constructor.
- `StockpilePriorityState` enum is defined locally in `StockpilePrioritySettings.cs`, not in the shared models file.
