# Extending Building Renovations

**Building Renovations** is both a gameplay mod and a framework. Other mods can add their own renovations without forking this project. The companion mod [**More Building Renovations**](../../MoreBuildingRenovations/) is the reference implementation for almost every pattern below.

A renovation has two halves that must stay in sync:

| Piece | Role |
|---|---|
| **`RenovationSpec` blueprint** | Display name, description, icon, group, order, materials, work days, free-form `Parameters` |
| **`RenovationBase` class** | Eligibility, soft unavailability, and the actual effect when work finishes |

The framework wires them together by **Id** at load time via `RenovationRegistry`.

---

## Lifecycle (what the framework already does)

For every finished building, `BuildingRenovationComponent` is present automatically (`[AddTemplateModule2(typeof(BuildingSpec))]`). When a player starts a renovation:

1. Materials are requested through Distro Storage (`RenovationDistroReceiver`). Science costs can be paid in the UI.
2. Once materials are fully stocked, work runs for `Spec.Days` (`RenovationWorkComponent`). Progress only counts after materials arrive.
3. On completion the renovation Id is stored in `ActiveRenovations` and **`RenovationBase.OnCompleted(building, isLoad: false)`** runs.
4. On save/load, every active Id is re-applied with **`isLoad: true`** so effects resume without re-charging materials.
5. Timed renovations inherit **`ExpirableRenovationBase`**: the framework tracks end time and calls **`OnExpired`** when duration ends.

Your extension usually only implements eligibility + effect logic. Material collection, UI, refunds, and persistence of “which renovations are active” are handled for you.

---

## Project setup

### 1. `manifest.json`

Depend on **BuildingRenovations** (and transitively its deps: ModdableTimberborn, DistroStorage, etc.):

```json
{
  "Name": "My Building Renovations",
  "Version": "1.0.0",
  "Id": "MyBuildingRenovations",
  "MinimumGameVersion": "1.1.1",
  "Description": "Extra renovations",
  "RequiredMods": [
    { "Id": "BuildingRenovations" }
  ]
}
```

See [`MoreBuildingRenovations/manifest.json`](../../MoreBuildingRenovations/manifest.json).

### 2. `.csproj`

Reference Building Renovations through the shared target (same pattern as More Building Renovations):

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputModFolderName>MyBuildingRenovations</OutputModFolderName>
  </PropertyGroup>

  <Import Project="../Targets/Common.targets" />
  <Import Project="../Targets/CommonRenovations.targets" />

  <ItemGroup>
    <Using Include="MyBuildingRenovations.Components" />
    <Using Include="MyBuildingRenovations.Renovations" />
  </ItemGroup>

</Project>
```

`CommonRenovations.targets` adds a reference to the installed BuildingRenovations DLL and global usings for:

- `BuildingRenovations`
- `BuildingRenovations.Components`
- `BuildingRenovations.Renovations`
- `BuildingRenovations.Services`
- `BuildingRenovations.Specs`
- `BuildingRenovations.Helpers`

### 3. Configuration entry point

Use ModdableTimberborn’s attribute configuration. Minimal (no extra systems):

```cs
namespace MyBuildingRenovations;

public class MConfigs : BaseModdableTimberbornAttributeConfiguration
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;
}
```

If renovations apply **worker bonuses** via ModdableTimberborn’s bonus tracker (as More Building Renovations does for workshops / haulers), enable it in `StartMod`:

```cs
public class MConfigs : BaseModdableTimberbornAttributeConfiguration, IModdableTimberbornRegistryWithPatchConfig
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;
    public string? PatchCategory => null;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseBonusTracker();
    }
}
```

See [`MoreBuildingRenovations/MConfigs.cs`](../../MoreBuildingRenovations/MConfigs.cs).

`[BindRenovation]` is enough for renovation classes; they are multi-bound as `RenovationBase` and injected into `RenovationRegistry` automatically. You do **not** need to register renovations by hand.

---

## Folder layout (recommended)

Mirror More Building Renovations:

```
MyBuildingRenovations/
  MConfigs.cs
  manifest.json
  Localizations/enUS.csv
  Renovations/                 # RenovationBase subclasses
  Components/                  # Optional effect components on buildings
  Services/                    # Optional helpers
  Blueprints/
    RenovationGroupSpecs/    # UI groups
    RenovationSpecs/         # Per-renovation specs (often one folder per group)
  UI/Renovations/            # Icons (PNG)
  Buildings/                 # Optional: attach ComponentSpecs to vanilla buildings
```

---

## Blueprint: groups

Groups control the section headers in the renovation dialog.

**Path:** `Root/Blueprints/RenovationGroupSpecs/<Name>.blueprint.json`

```json
{
  "RenovationGroupSpec": {
    "Id": "Agriculture",
    "TitleLoc": "LV.MBR.AgricultureGroup",
    "Order": 15
  }
}
```

| Field | Meaning |
|---|---|
| `Id` | Group key. Renovation specs reference this via `GroupId`. |
| `TitleLoc` | Localization key for the group title. |
| `Order` | Sort order among groups (lower first). |

You can reuse an existing group Id from Building Renovations (e.g. `Stockpile`) or define a new one.

---

## Blueprint: renovation specs

```json
{
  "RenovationSpec": {
    "Id": "HarvestSpeed1",
    "GroupId": "Agriculture",
    "Order": 30,
    "TitleLoc": "LV.MBR.HarvestSpeed1",
    "DescLoc": "LV.MBR.HarvestSpeed1Desc",
    "FlavorLoc": "LV.MBR.HarvestSpeed1Flavor",
    "Icon": "UI/Renovations/HarvestSpeed1",
    "Cost": [
      { "Id": "Plank", "Amount": 4 }
    ],
    "Days": 0.5,
    "Parameters": [ 0.25, 10 ]
  }
}
```

### Field reference

| Field | Required | Meaning |
|---|---|---|
| `Id` | yes | Must match `RenovationBase.Id` exactly. |
| `GroupId` | yes | Existing `RenovationGroupSpec.Id`. |
| `Order` | yes | Sort order within the group. |
| `TitleLoc` | yes | Localization key for the title. |
| `DescLoc` | no | Description key. Formatted with `Parameters` as `{0}`, `{1}`, …; token `[Days]` is replaced with work duration. |
| `FlavorLoc` | yes (can be `""`) | Flavor text under the description. |
| `Icon` | no | Asset path under your mod’s UI folder, without extension (e.g. `UI/Renovations/HarvestSpeed1` → `Root/UI/Renovations/HarvestSpeed1.png`). |
| `Cost` | yes | Array of `{ "Id", "Amount" }`. Use `"Science"` for science points. Empty array if cost is fully custom (see below). |
| `CustomCodeCost` | no | If `true`, cost comes from `ICustomCostRenovation.GetCost()` instead of `Cost`. |
| `Days` | yes | Work duration in days after materials are collected. |
| `Parameters` | no | Free-form floats for balance values (buff strength, duration, etc.). Read as `Spec.Parameters[i]` in code. |

### Description formatting

`DescLoc` is resolved as:

```text
string.Format(T(DescLoc), Parameters[0], Parameters[1], …).Replace("[Days]", Days)
```

Example loc entry:

```csv
LV.MBR.HarvestSpeed1Desc,"{0:+0%} Harvesting Speed for {1:0} days",
```

With `"Parameters": [ 0.25, 10 ]` this becomes **+25% Harvesting Speed for 10 days**.

---

## Code: `RenovationBase`

### Registration

```cs
[BindRenovation]
public class MyRenovation : RenovationBase
{
    public override string Id => nameof(MyRenovation);
    // ...
}
```

- `[BindRenovation]` multi-binds the type as `RenovationBase`.
- Use `[BindRenovation(AlsoBindSelf = true)]` if something else must inject the **concrete** type as well (see `ObservatoryPassive`).

### Members you implement

```cs
public abstract class RenovationBase
{
    public abstract string Id { get; }

    // Assigned by RenovationRegistry from the matching RenovationSpec
    public RenovationSpec Spec { get; }

    // Hard filter: false → hidden unless "Show unavailable renovations" is on
    public abstract bool CanRenovate(BuildingRenovationComponent building);

    // Soft filter: non-null → always listed but grayed; message shown on click
    public virtual string? GetUnavailableReason(BuildingRenovationComponent building) => null;

    // Extra lines under the main description in the detail panel
    public virtual string? GetExtraDescription(BuildingRenovationComponent building) => null;

    // Whole-game filter: missing goods, missing building types for this faction, etc.
    public virtual bool IsAvailableToThisGame(RenovationRegistry registry) { /* default: cost goods exist */ }

    // Apply effect. isLoad == true when re-applying after save load.
    public abstract void OnCompleted(BuildingRenovationComponent building, bool isLoad);
}
```

**Rules of thumb**

| Method | Use for |
|---|---|
| `CanRenovate` | Wrong building type / missing component → not applicable at all |
| `GetUnavailableReason` | Prerequisite renovations, weaker tier already active, temporary blocks |
| `IsAvailableToThisGame` | Faction-only buildings, goods that do not exist in this run |

Already-active renovations are handled by `BuildingRenovationComponent` (shows “already active”); you usually do not re-check that in `GetUnavailableReason`.

### Permanent renovation (minimal)

```cs
[BindRenovation]
public class BuilderHaulerSpeedBoost(ILoc t) : RenovationBase
{
    public override string Id => nameof(BuilderHaulerSpeedBoost);

    public override bool CanRenovate(BuildingRenovationComponent building)
        => building.HasComponent<HaulingCenterSpec>() || building.HasComponent<BuilderHubSpec>();

    public override void OnCompleted(BuildingRenovationComponent building, bool isLoad)
    {
        ImmutableArray<BonusSpec> bonuses = [
            BonusType.CarryingCapacity.ToBonusSpec(Spec.Parameters[0]),
            BonusType.MovementSpeed.ToBonusSpec(Spec.Parameters[1]),
        ];
        var desc = t.TWorkplaceWorkerBonus(bonuses);

        building.GetComponent<WorkplaceBonuses>().AddBonus(new(
            bonuses,
            new(
                $"Renovation_{Id}",
                Spec.Title.Value,
                desc
            )
        ));
    }
}
```

Source: [`BuilderHaulerSpeedBoost.cs`](../../MoreBuildingRenovations/Renovations/BuilderHaulerSpeedBoost.cs).

### Tier chains (prerequisite renovations)

Pass the previous tier Id and use `BuildingRenovationService.GetRequiredRenovationIdReason`:

```cs
public abstract class WoodBuildingsImprovement(string id, ILoc t, string? requiredId = null) : RenovationBase
{
    public override string Id => id;

    public override string? GetUnavailableReason(BuildingRenovationComponent building)
        => building.Service.GetRequiredRenovationIdReason(building, requiredId);

    // CanRenovate + OnCompleted ...
}

[BindRenovation]
public class WoodBuildingsImprovement1(ILoc t) : WoodBuildingsImprovement(nameof(WoodBuildingsImprovement1), t);

[BindRenovation]
public class WoodBuildingsImprovement2(ILoc t)
    : WoodBuildingsImprovement(nameof(WoodBuildingsImprovement2), t, nameof(WoodBuildingsImprovement1));
```

Source: [`WoodBuildingsImprovement.cs`](../../MoreBuildingRenovations/Renovations/WoodBuildingsImprovement.cs), [`IncreaseDistroSpeed.cs`](../../BuildingRenovations/Renovations/IncreaseDistroSpeed.cs).

### Filtering by template name

```cs
static readonly ImmutableArray<string> TemplatePrefixes = ["LumberMill.", "GearWorkshop."];

public override bool CanRenovate(BuildingRenovationComponent building)
    => building.TemplateStartsWith(TemplatePrefixes);
```

`TemplateStartsWith` lives on `RenovationHelpers` (extension on `BaseComponent`).

### Faction / content availability

Hide renovations when the relevant building does not exist in this game:

```cs
public override bool IsAvailableToThisGame(RenovationRegistry registry)
    => registry.HasAnyBuildingWith<NumbercruncherRenovationSpec>()
       && base.IsAvailableToThisGame(registry);
```

`HasAnyBuildingWith<T>()` checks **template specs** (available early, before entities spawn). The default `IsAvailableToThisGame` also drops renovations whose cost goods are missing (except Science).

---

## Timed renovations: `ExpirableRenovationBase`

Use this when the buff should expire after a duration.

```cs
public abstract class ExpirableRenovationBase : RenovationBase
{
    // Default: Spec.Parameters[0] as duration in days
    public virtual float GetDurationDays(BuildingRenovationComponent building);

    // Apply timed effect (called for fresh complete and load resume)
    protected virtual void OnActivated(BuildingRenovationComponent building, bool isLoad) { }

    // Called when the timer ends (active flag already cleared)
    public virtual void OnExpired(BuildingRenovationComponent building) { }
}
```

**Do not override `OnCompleted`** on expirable renovations — it is sealed. Put apply logic in `OnActivated` and cleanup in `OnExpired`.

### Duration conventions

| Pattern | Example |
|---|---|
| Duration in `Parameters[0]` (default) | Simple timed buff |
| Bonus in `[0]`, duration days in `[1]` | `FarmingSpeedBase` overrides `GetDurationDays` → `Parameters[1]` |
| Duration stored in hours | `InventorExtraScience`: `GetDurationDays` → `Parameters[0] / 24f` |

### Example: timed farmhouse speed

```cs
public abstract class FarmingSpeedBase(string id, ILoc t) : ExpirableRenovationBase
{
    public override string Id => id;
    public float SpeedBonus => Spec.Parameters[0];
    public override float GetDurationDays(BuildingRenovationComponent building) => Spec.Parameters[1];

    public override bool CanRenovate(BuildingRenovationComponent building)
        => building.HasComponent<FarmhouseActionSpeed>();

    protected override void OnActivated(BuildingRenovationComponent building, bool isLoad)
    {
        building.GetComponent<BonusDescriptionComponent>().AddBonus(new(
            "Renovation_" + Id,
            Spec.Title.Value,
            /* description */,
            _ => building.Expirable.GetRemainingHoursOrNull(Id)  // remaining time in UI
        ));

        // Apply gameplay effect...
    }

    public override void OnExpired(BuildingRenovationComponent building)
    {
        building.GetComponent<BonusDescriptionComponent>().RemoveBonus("Renovation_" + Id);
        // Clear gameplay effect if no stronger family tier remains...
        base.OnExpired(building);
    }
}
```

Source: [`FarmingSpeedBase.cs`](../../MoreBuildingRenovations/Renovations/FarmingSpeedBase.cs), [`InventorExtraScience.cs`](../../MoreBuildingRenovations/Renovations/InventorExtraScience.cs).

### Expiry helpers on the building

```cs
building.Expirable.GetRemainingHoursOrNull(id);
building.Expirable.TryGetRemainingDays(id, out var days);
building.Expirable.Cancel(id);              // stop timer without OnExpired
building.OnRenovationExpired(id);           // full expire path (clears active + OnExpired)
building.DeactivateRenovation(id);          // remove active without expire callbacks
```

---

## Custom material costs: `ICustomCostRenovation`

When cost depends on game content (e.g. every plant product), set `"CustomCodeCost": true` and implement the interface.

**Blueprint:**

```json
{
  "RenovationSpec": {
    "Id": "PlantsSpeed",
    "CustomCodeCost": true,
    "Cost": [],
    "Days": 3,
    "Parameters": [ 0.25 ]
    // TitleLoc, GroupId, ...
  }
}
```

**Code:**

```cs
[BindRenovation]
public class PlantsSpeed(ILoc t, TemplateService templateService)
    : RenovationBase, ICustomCostRenovation, ILoadableSingleton
{
    ImmutableArray<GoodAmount> cost = [];

    public void Load()
    {
        // Build cost from templates...
        cost = [/* ... */];
    }

    public IEnumerable<GoodAmount> GetCost() => cost;

    // CanRenovate + OnCompleted ...
}
```

Source: [`PlantsSpeed.cs`](../../MoreBuildingRenovations/Renovations/PlantsSpeed.cs).

Registry validation enforces: `CustomCodeCost == true` **iff** the class implements `ICustomCostRenovation`.

---

## Showing effects on the building panel

Every building already has `BonusDescriptionComponent`. Register a named buff so the entity panel lists it:

```cs
building.GetComponent<BonusDescriptionComponent>().AddBonus(new(
    id: "Renovation_" + Id,
    title: Spec.Title.Value,
    description: "…",
    remainingHours: _ => building.Expirable.GetRemainingHoursOrNull(Id) // optional, timed only
));

// Later (expire / deactivate):
building.GetComponent<BonusDescriptionComponent>().RemoveBonus("Renovation_" + Id);
```

`BonusDescription` also accepts `Func<ILoc, IDayNightCycle, string>` for dynamic text.

Worker-facing bonuses often go through a small helper component (see `WorkplaceBonuses` in More Building Renovations) that applies ModdableTimberborn `BonusTracker` entries to assigned workers **and** registers the description.

Shared loc helper for worker buffs:

```cs
t.TWorkplaceWorkerBonus(bonuses); // "Workers gain +50% Working Speed, ..."
```

---

## Effect components on buildings

Put non-trivial runtime state on a dedicated component rather than inside the renovation class.

### Attach via `AddTemplateModule2` (all buildings of a type)

```cs
// Every FarmHouse gets this component automatically
[AddTemplateModule2(typeof(FarmHouse))]
public class FarmhouseActionSpeed : BaseComponent
{
    public float HarvestSpeedBonus { get; private set; }
    public void SetHarvestSpeed(float bonus) => HarvestSpeedBonus = bonus;
    public void ClearHarvestSpeed() => HarvestSpeedBonus = 0f;
}
```

### Attach via building blueprint + marker spec (selected buildings only)

1. Define an empty (or configured) `ComponentSpec`:

```cs
public record InventorRenovationSpec : ComponentSpec;

[AddTemplateModule2(typeof(InventorRenovationSpec))]
public class InventorRenovationBonus(ScienceService scienceService) : BaseComponent, IAwakableComponent
{
    // ...
}
```

2. Drop a blueprint that merges the spec onto the vanilla building:

`Root/Buildings/Science/Inventor/Inventor.Folktails.blueprint.json`:

```json
{
  "InventorRenovationSpec": {}
}
```

Repeat for each faction / variant template that should get the renovation. Examples live under [`MoreBuildingRenovations/Root/Buildings/`](../../MoreBuildingRenovations/Root/Buildings/).

Then in the renovation:

```cs
public override bool CanRenovate(BuildingRenovationComponent building)
    => building.HasComponent<InventorRenovationBonus>();
```

---

## Localization

Add a `Localizations/enUS.csv` (and other languages as needed):

```csv
ID,Text,Comment
LV.MBR.AgricultureGroup,"Agriculture",
LV.MBR.HarvestSpeed1,"Wooden Flail",
LV.MBR.HarvestSpeed1Desc,"{0:+0%} Harvesting Speed for {1:0} days",
LV.MBR.HarvestSpeed1Flavor,"It won't last forever, but while the shafts hold, the beavers swing it with joy.",
LV.MBR.HarvestSpeedBuff,"{0:+0%} Harvesting Speed.",
```

Typical keys per renovation:

- Group title (`TitleLoc` on the group)
- Renovation title (`TitleLoc`)
- Description (`DescLoc`) with `{0}`… from `Parameters`
- Flavor (`FlavorLoc`)
- Optional buff line used when registering `BonusDescription`

Prefix keys with your mod’s short tag (e.g. `LV.MBR.`) to avoid collisions.

---

## Icons

Place PNGs at:

```text
Root/UI/Renovations/<Name>.png
```

Reference them from the blueprint **without** the extension:

```json
"Icon": "UI/Renovations/HarvestSpeed1"
```

---

## Helpers worth knowing

From `RenovationHelpers` / services (already imported via `CommonRenovations.targets`):

| API | Purpose |
|---|---|
| `building.GetRenovationComponent()` | Get `BuildingRenovationComponent` from any component on the building |
| `building.TemplateStartsWith(prefixes)` | Template-name filter |
| `registry.HasAnyBuildingWith<TSpec>()` | Content exists in this game |
| `registry.ContainsAllGoods(...)` | Cost goods exist |
| `building.Service.GetRequiredRenovationIdReason(building, requiredId)` | Prerequisite renovation message |
| `building.HasActive(id)` | Whether Id is currently active |
| `building.Service.PartialDayNumber` | Current partial day (for custom timers) |
| `t.TWorkplaceWorkerBonus(...)` | Localized “Workers gain …” string |

---

## End-to-end checklist

1. **Mod shell** — `manifest.json` requires `BuildingRenovations`; `.csproj` imports `CommonRenovations.targets`; `MConfigs` configures the game context (and optional systems).
2. **Group** — `Root/Blueprints/RenovationGroupSpecs/*.blueprint.json` (or reuse an existing group Id).
3. **Spec** — `Root/Blueprints/RenovationSpecs/.../*.blueprint.json` with unique `Id`, costs, `Days`, `Parameters`.
4. **Effect class** — `[BindRenovation]` subclass of `RenovationBase` or `ExpirableRenovationBase`; `Id` matches the blueprint.
5. **Eligibility** — implement `CanRenovate` (and optionally `GetUnavailableReason` / `IsAvailableToThisGame`).
6. **Effect** — implement `OnCompleted` or `OnActivated`/`OnExpired`; re-apply safely when `isLoad` is true.
7. **UI feedback** — register/remove `BonusDescription` entries.
8. **Components** — add any needed building components via `AddTemplateModule2` and/or building blueprints.
9. **Loc + icon** — CSV keys and PNG under `Root/UI/Renovations/`.
10. **Custom cost** (optional) — `CustomCodeCost: true` + `ICustomCostRenovation`.

---

## Worked references in More Building Renovations

| Goal | Look at |
|---|---|
| Permanent worker bonuses on filtered buildings | `BuilderHaulerSpeedBoost`, `WoodBuildingsImprovement` |
| Tier prerequisites | `WoodBuildingsImprovement*`, core `IncreaseDistroSpeed*` |
| Timed buff with family tiers | `FarmingSpeedBase`, `HarvestSpeed`, `PlantingSpeed` |
| Timed buff with hour-based duration | `InventorExtraScience` |
| Custom dynamic cost | `PlantsSpeed` |
| Marker-spec building attachment | `InventorRenovationBonus` + `Root/Buildings/Science/Inventor/` |
| Faction / template availability | `NumbercruncherSubmerge`, `ObservatoryPassive` |
| Extra description lines | `PlantsSpeed.GetExtraDescription` |
| Project wiring | [`MoreBuildingRenovations.csproj`](../../MoreBuildingRenovations/MoreBuildingRenovations.csproj), [`MConfigs.cs`](../../MoreBuildingRenovations/MConfigs.cs), [`manifest.json`](../../MoreBuildingRenovations/manifest.json) |

---

## Common pitfalls

- **Id mismatch** — Registry throws if a `[BindRenovation]` class has no matching `RenovationSpec.Id` (or the reverse is silently unused).
- **Forgetting re-apply on load** — `OnCompleted` / `OnActivated` run with `isLoad: true` after load. Apply modifiers idempotently (set values; do not double-subscribe events without unsubscribing first).
- **Expirable cleanup** — Always remove bonuses / reverse effects in `OnExpired`. Active flag is already cleared before `OnExpired` runs.
- **`CustomCodeCost` mismatch** — Must implement `ICustomCostRenovation` exactly when the flag is true.
- **Hard vs soft filter** — Wrong building → `CanRenovate == false`. Missing prerequisite → `GetUnavailableReason`. Using the wrong one confuses the “show unavailable” UI.
- **Science goods** — Cost Id `"Science"` is special-cased; it is not a normal good in `IGoodService`.
- **Work time vs buff duration** — `Spec.Days` is only the **construction** duration. Buff length for timed renovations comes from `GetDurationDays` / `Parameters`, not from `Days`.
