# Extra Hauler Targets

Lets mods register **extra inventories** as destinations haulers can fill — without those entities being finished `HaulCandidate` buildings (the vanilla haul pipeline only tracks finished haul candidates).

Primary use case: unfinished **construction sites** (including multi-access / no-entrance buildings such as platforms). The [Construction Site Hauler](../../ConstructionSiteHauler/) mod is the reference consumer.

**Relevant source:** [`../../ModdableTimberborn/HaulingSystem/`](../../ModdableTimberborn/HaulingSystem/)

## Why this exists

Vanilla hauling:

1. Only **finished** buildings with `HaulCandidate` register in `DistrictHaulCandidates`.
2. Fill jobs (e.g. `FillInputWorkplaceBehavior`) call `CarrierInventoryFinder.TryCarryFromAnyInventory`, which looks up stock using the **receiver’s** `DistrictBuilding.District`.
3. Unfinished construction sites usually have **no** `District` (only `ConstructionDistrict` when they have an entrance / `DistrictBuilding`), and use **multi-access** `ConstructionSiteAccessible` — not a single entrance.

Builders still work because they use a **global** construction job list and path from the builders’ hut. Haulers never saw those sites. This API plugs extra destinations into the same **weighted haul job list** haulers already use, and fixes stock search / multi-access pathing for those targets.

## Enabling the subsystem

```cs
public override void StartMod(IModEnvironment modEnvironment)
{
    base.StartMod(modEnvironment);

    ModdableTimberbornRegistry.Instance.UseExtraHaulerTargets();
}
```

Idempotent and chainable. **Game context only.**

Enabling:

- Binds `ExtraHaulerTargetService` as a singleton.
- Applies Harmony patches (category `ModdableTimberborn.HaulingSystem`).
- Sets `ExtraHaulerTargetService.Instance` on load (cleared on unload) for hot-path access.

`HaulingTargetHelper` is a separate `[BindSingleton]` helper available whenever ModdableTimberborn loads attributes (not gated solely on this enabler).

## Key types

| Type | Purpose |
|------|---------|
| [`ExtraHaulerTargetService`](../../ModdableTimberborn/HaulingSystem/ExtraHaulerTargetService.cs) | Register / remove / prioritize extra destinations |
| [`ExtraHaulerTargetRegistration`](../../ModdableTimberborn/HaulingSystem/ExtraHaulerTargetRegistration.cs) | Immutable registration payload (`readonly record struct`) |
| [`HaulingTargetHelper`](../../ModdableTimberborn/HaulingSystem/HaulingTargetHelper.cs) | Resolve which `DistrictCenter`s should serve an `Accessible` |

### Registration payload

```cs
public readonly record struct ExtraHaulerTargetRegistration(
    Inventory Inventory,
    IReadOnlyList<DistrictCenter> Districts,
    Accessible? Accessible = null,
    float Weight = 1f,
    bool OnlyInputGoods = true
);
```

| Field | Meaning |
|-------|---------|
| `Inventory` | Destination inventory haulers may fill |
| `Districts` | Hauling posts in these districts may be **offered** the job |
| `Accessible` | Optional walk target; pathing still uses the inventory entity’s enabled `Accessible` in vanilla carry |
| `Weight` | Base haul job weight (sorted with normal haul candidates; higher first) |
| `OnlyInputGoods` | Documented for callers; fill uses vanilla `FillInputWorkplaceBehavior` (input goods) |

## Requirements for a destination

1. **`FillInputWorkplaceBehavior`** on the same entity as the inventory (vanilla fill job).  
   - Your mod must add this decorator if the template does not already have it (e.g. construction sites).
2. An enabled **`Accessible`** with valid access points (single entrance or multi-access).
3. At least one **`DistrictCenter`** in `Districts` (finished districts with a hauling post).

If `FillInputWorkplaceBehavior` is missing, `AddExtraTarget` throws a clear `InvalidOperationException`.

## Basic usage

```cs
// Prefer HaulingTargetHelper for district resolution
var districts = haulingTargetHelper
    .FindDistrictsFor(siteAccessible)
    .ToArray();

if (districts.Length == 0)
{
    return; // not assigned / not on any road spill yet
}

var handle = extraHaulerTargets.AddExtraTarget(new ExtraHaulerTargetRegistration(
    Inventory: inventory,
    Districts: districts,
    Accessible: siteAccessible,
    Weight: 1.1f));

// When done (finished, deleted, full, nav no longer reaches):
handle.Dispose();
// or: extraHaulerTargets.RemoveExtraTarget(inventory);
```

**Persistence:** the service does **not** save registrations. Re-register after load (e.g. on unfinished enter / inventory change / nav refresh).

### Prioritize flag (vanilla-style +0.5)

```cs
extraHaulerTargets.SetExtraTargetPrioritized(inventory, prioritized: true);
// No-op if not currently registered
```

Vanilla formula (same as `HaulCandidate`): if prioritized **and** base weight ≥ 0.5 → effective weight = weight + 0.5.

You can also put priority entirely into `Weight` and leave prioritize false (recommended for multi-level construction priority).

## HaulingTargetHelper — which districts?

```cs
IEnumerable<DistrictCenter> FindDistrictsFor(
    Accessible accessible,
    bool ignoreEntrance = false);
```

| Mode | Behavior |
|------|----------|
| `ignoreEntrance: false` (default) | If the entity has `DistrictBuilding` with an assigned finished/construction district → return that one. If it has `DistrictBuilding` but unassigned → empty (wait). If **no** `DistrictBuilding` (no entrance) → road-spill scan. |
| `ignoreEntrance: true` | Always road-spill scan. |

**Road spill** uses precomputed instant district maps (`InstantDistrictMap.TryGetParentRoadNode`) — the same family of data as builder construction reachability / district range, **not** a full A* per district.

Also available:

- `FindDistrictsByRoadSpill(Accessible)`
- `IsAccessibleOnDistrictSpill(District, Accessible)`

Return type is `IEnumerable` so callers can use `FirstOrDefault()`; materialize with `ToArray()` / `ToList()` when you need `Count` or a stable list for registration.

### District building vs spill (important)

```text
Entrance → BuildingAccessible → DistrictBuilding
                → District (finished)
                → ConstructionDistrict (unfinished, when assigned)
```

Platforms / roofs without entrance **never** get `DistrictBuilding`, so they never get a construction district. Builders still find them via a **global** construction job list + path from the hut. For haulers, use spill-based `FindDistrictsFor` so only districts whose road network “covers” the site are offered the job (avoids registering every district on the map).

## How jobs and stock work (mental model)

```text
Registration.Districts     → which hauling posts may attempt the job
Hauler workplace.District  → where stock is searched (after pick up)
Weight (+ optional prioritize) → sort order vs warehouses / workshops / other extras
FillInputWorkplaceBehavior → vanilla fill decide
CarryRootBehavior          → walk + deliver (unchanged)
```

### Weight tips

Workshop empty fill weight ≈ **1.0** (`1 − fill%`).

If construction weight **falls** as the site fills, haulers abandon half-built sites for empty factories. Prefer **fixed weights** by importance while the inventory still needs goods, e.g.:

| Priority (example) | Weight | Role |
|--------------------|--------|------|
| VeryLow | 0.25 | Below empty workshop |
| Low | 0.5 | Weak vs empty workshops |
| Normal | 1.1 | Above empty workshop; keeps feeding until full |
| High | 1.5 | Between important jobs |
| VeryHigh | 2.0 | Top tier |

Unregister when full so weight does not matter after completion.

## Harmony patches (applied with the enabler)

| Patch | Role |
|-------|------|
| `DistrictHaulCandidates.GetWorkplaceBehaviorsOrdered` | Merges registered fill jobs into the same weighted sort as vanilla haul candidates |
| `CarrierInventoryFinder.TryCarryFromAnyInventoryInternal` | Stock district = **hauler workplace district** (transpiler replaces district assignment) |
| `DistrictInventoryPicker.ClosestInventoryWithStock(Accessible, …)` | Multi-access destinations: path **warehouse → site** (avoids `UnblockedSingleAccess` / `.Single()` crash on construction sites) |

Do not add a separate workplace behavior on the hauling post for extras — that would run entirely before/after all normal haul jobs and break weight ordering.

## Minimal consumer checklist

1. `UseExtraHaulerTargets()` in your mod starter / registry config.  
2. Ensure destination has `FillInputWorkplaceBehavior` (template decorator).  
3. Resolve districts (`HaulingTargetHelper` or your own list).  
4. `AddExtraTarget` when the inventory should receive goods; `Dispose` / `Remove` when not.  
5. Re-register on load and when reachability or priority changes (nav, district, inventory full, etc.).  
6. Choose `Weight` carefully relative to workshop fill (~1.0 empty).

Reference implementation: **Construction Site Hauler** (`ConstructionSiteHauler/`) — construction sites, spill districts, builder-priority weights, optional “disable builder hauling” toggles.

## Dependencies & patterns

| Piece | Source |
|-------|--------|
| Opt-in `UseExtraHaulerTargets()` | `ModdableTimberbornRegistry` partial in `ExtraHaulerTargetConfig` |
| DI | `ExtraHaulerTargetService`, `HaulingTargetHelper` (`[BindSingleton]`) |
| Harmony | Category `ModdableTimberborn.HaulingSystem` |
| Vanilla fill | `FillInputWorkplaceBehavior` |
| District maps | `InstantDistrictMap`, `NodeIdService`, `DistrictCenterRegistry` |

## Gotchas

- **Registrations are not saved** — re-register after load.  
- **`Districts` ≠ stock district** — stock always comes from the **hauler’s** district once the job is taken.  
- **No entrance** → no `DistrictBuilding` → use spill-based district lists; do not assume `ConstructionDistrict` exists.  
- **Multi-access** destinations need the inventory-picker patch (enabled with this subsystem).  
- **Equal weights** among many sites do not guarantee finish-one-then-next; haulers can feed several in parallel. Prefer fixed Normal+ weights if you want completion over drip-feeding every site equally.  
- Hot path: prefer `ExtraHaulerTargetService.Instance` over container lookup when patches need the service.
