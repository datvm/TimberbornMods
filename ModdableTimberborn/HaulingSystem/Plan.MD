# Extra Hauler Targets — Design Plan

Design doc for a **ModdableTimberborn** hauling extension: let mods register **extra inventories** as destinations haulers may fill, without turning those entities into normal finished `HaulCandidate` buildings.

**Status:** design only (not implemented).  
**Folder:** `ModdableTimberborn/HaulingSystem/`  
**Primary example:** allow haulers to bring **construction materials to unfinished construction sites**.

---

## 1. Problem

### 1.1 What haulers do today

1. `HaulWorkplaceBehavior` asks the district’s `DistrictHaulCandidates` for ordered jobs.  
2. Only **finished** buildings with `HaulCandidate` are registered.  
3. Jobs (e.g. `FillInputWorkplaceBehavior`) call `CarrierInventoryFinder.TryCarryFromAnyInventory`.  
4. That finder uses **`receivingInventory.DistrictBuilding.District`** to find source stock.  
5. After reservation, `CarryRootBehavior` walks to each inventory’s **`Accessible`** and delivers.

### 1.2 Why unfinished construction sites never get haulers

| Requirement | Unfinished construction site |
|-------------|------------------------------|
| In `HaulCandidate` list | **No** — only finished buildings register |
| `District` set | **Usually no** — has `ConstructionDistrict` instead |
| `TryCarryFromAnyInventory` | **Fails** — `District` is null, never finds stock |
| Public input inventory | Construction inventory is **not** public input |
| Walk destination | **OK** — `ConstructionSiteAccessible` (surrounding + above), no entrance needed |

**Important:** haulers do **not** fail because of missing entrances. Construction access is multi-point (`ConstructionSiteAccessible`). Once stock + capacity are reserved, **`CarryRootBehavior` can already walk to the site** the same way builders do after `ConstructionJob` reserves.

The gap is **job discovery + stock reservation**, not pathing.

### 1.3 How builders succeed (pattern to copy for haulers)

`ConstructionJob`:

1. Uses the **builder hub’s district** (workplace), not the site’s `District`.  
2. Finds stock in that district.  
3. Reserves stock + reserves capacity on the **construction site inventory**.  
4. Releases next tick → `CarryRootBehavior` delivers.

Extra hauler targets should use the **same reservation shape**, with the hauling post’s district as the stock source scope.

---

## 2. Goals

| Goal | Description |
|------|-------------|
| General MT API | Any mod can register/unregister extra haul **destinations** |
| District-scoped | Only haulers in listed districts consider a target |
| No fake buildings | No need for finished `HaulCandidate`, entrance, or public warehouse semantics |
| Reuse carry | After reserve, vanilla `CarryRootBehavior` walks and delivers |
| Construction example | Unfinished CS inventory can receive materials from haulers |
| Explicit lifecycle | Caller adds on start, removes on finish/cancel/delete |

## 3. Non-goals (v1)

- Replacing or reimplementing all of hauling  
- Auto-registering every vanilla construction site (callers opt in)  
- Hauler UI toggles / batch control for extra targets  
- Persisting registrations across save (owners re-register on load)  
- Fully undistricted “any hauler on the map” (optional later)  
- Changing builder construction jobs  
- Science / refunds / DCS (separate systems; they may *use* this API)

---

## 4. Public API

### 4.1 Service

Concrete service (no interface): **`ExtraHaulerTargetService`**.

```csharp
// ExtraHaulerTargetService
IDisposable AddExtraTarget(ExtraHaulerTargetRegistration registration);
void RemoveExtraTarget(Inventory inventory);
void SetExtraTargetPrioritized(Inventory inventory, bool prioritized); // no-op if missing
bool IsExtraTargetPrioritized(Inventory inventory);
IReadOnlyList<ExtraHaulerTargetRegistration> GetTargetsForDistrict(DistrictCenter district);
```

**Prioritized state** is **mutable service state** keyed by `Inventory`, not a field on the registration struct (registration stays immutable).  
Default when first added: `prioritized = false`.

Vanilla mirror (`HaulCandidate.PrioritizeAndValidate`):

```text
if (Prioritized && weight >= 0.5)
    effectiveWeight = weight + 0.5   // PriorityFactor
else
    effectiveWeight = weight
```

Extra targets use the **same formula** so they sort fairly with normal haul jobs.

### 4.2 Registration

```csharp
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
| `Districts` | Hauling posts in these districts may serve it |
| `Accessible` | Walk target; if `null`, `Inventory.GetEnabledComponent<Accessible>()` |
| `Weight` | Base weight before prioritize boost (~0..1+, same spirit as `WeightedBehavior`) |
| `OnlyInputGoods` | If true, only `InputGoods` with capacity; else allowed goods with capacity |

### 4.3 Enabling

```csharp
ModdableTimberbornRegistry.Instance.UseExtraHaulerTargets();
// Game context only
```
---

## 5. Example: haulers bring materials to a construction site

```csharp
// When an unfinished ConstructionSite should accept hauler deliveries:
var site = constructionSite; // Timberborn.ConstructionSites.ConstructionSite
var inventory = site.Inventory;
var accessible = site.GetComponentFast<ConstructionSiteAccessible>().Accessible;
var district = site.GetComponentFast<DistrictBuilding>().ConstructionDistrict
    ?? site.GetComponentFast<DistrictBuilding>().District;

if (district is null)
{
    // not on construction/district road network yet — skip or retry
    return;
}

_registration = extraHaulerTargets.AddExtraTarget(new ExtraHaulerTargetRegistration(
    Inventory: inventory,
    Districts: [district],
    Accessible: accessible,
    Weight: 0.85f, // base weight; compete with other haul jobs
    OnlyInputGoods: true // construction cost goods are givable → InputGoods
));

// Optional: same boost as HaulPrioritizable on buildings
extraHaulerTargets.SetExtraTargetPrioritized(inventory, prioritized: true);
// Not registered? no-op, does not throw

// On site finished, deleted, or mod disables the feature:
_registration.Dispose();
// or: extraHaulerTargets.RemoveExtraTarget(inventory);
```

**Save/load:** do **not** rely on the service to persist. On load, when the site enters unfinished (or mod component loads), call `AddExtraTarget` again.

**Builders + haulers together:** both may reserve **unreserved** capacity/stock. That is intentional and matches vanilla reservation safety. No need to disable builder supply unless a separate feature wants that.

---

## 6. How it plugs into hauling (implementation)

### 6.1 Why not a separate HaulingCenter WorkplaceBehavior

`WorkerRootBehavior` runs workplace behaviors in decorator order. A dedicated “extra targets” behavior would run **entirely before or after** `HaulWorkplaceBehavior`, so:

- either extras always beat **all** normal haul jobs, or  
- extras always lose to **all** of them  

That **breaks** vanilla weight + prioritize ordering (`HaulCandidate` → `WeightedBehavior` sorted by weight, prioritize adds `+0.5` when `Prioritized && weight >= 0.5`).

**Decision:** do **not** add a sibling workplace behavior for extras. Inject into the **same weighted list** as normal haul candidates.

### 6.2 Vanilla order (what we must join)

```text
HaulWorkplaceBehavior.Decide
  → HaulingCenter.GetWorkplaceBehaviorsOrdered
    → DistrictHaulCandidates.GetWorkplaceBehaviorsOrdered
         for each HaulCandidate:
           providers → WeightedBehavior(weight, WorkplaceBehavior)
           if prioritized && weight >= 0.5: weight += 0.5
         sort weighted by weight DESC
         emit WorkplaceBehavior list in that order
  → try each WorkplaceBehavior until one takes the agent
```

`GetWorkplaceBehaviorsOrdered` **discards weights** after sort — it only returns ordered `WorkplaceBehavior`s. So a **postfix that only appends** to that list cannot re-sort fairly against vanilla weights.

### 6.3 Harmony approach (recommended): Prefix rewrite of a short method — not a Transpiler

**Target:** `DistrictHaulCandidates.GetWorkplaceBehaviorsOrdered`

```csharp
// Conceptual
[HarmonyPrefix]
[HarmonyPatch(typeof(DistrictHaulCandidates), nameof(DistrictHaulCandidates.GetWorkplaceBehaviorsOrdered))]
static bool Prefix(DistrictHaulCandidates __instance, IList<WorkplaceBehavior> workplaceBehaviors)
{
    // 1) Same as vanilla: collect WeightedBehavior from all HaulCandidates
    // 2) Append WeightedBehavior for each ExtraHaulerTarget in this district
    //      effectiveWeight = ApplyPrioritize(registration.Weight, isPrioritized)
    //      WorkplaceBehavior = shared ExtraHaulFillBehavior bound to that Inventory
    // 3) Sort by weight DESC (vanilla)
    // 4) Fill workplaceBehaviors list
    // 5) return false; // skip original
}
```

| Option | Difficulty | Priority-correct? |
|--------|------------|-------------------|
| **Prefix + reimplement method (~15 lines vanilla + extras)** | **Easy–Medium** | **Yes** |
| Postfix append-only to ordered list | Easy | **No** (extras always last) |
| Extra `WorkplaceBehavior` on `HaulingCenter` | Easy | **No** (all-before / all-after) |
| **Transpiler** inject before `Sort` | Medium–Hard, brittle | Yes if done right |
| Full replace via Postfix clear+rebuild | Easy–Medium | Yes (same as prefix) |

**Transpiler:** not needed. The vanilla method is tiny (collect → sort → project). A **Prefix that skips original** and copies that logic is clearer, easier to maintain, and avoids IL fragility across game updates.

**Difficulty:** low–medium. Hard part is **not** Harmony; it is the **per-target fill `WorkplaceBehavior`** that reserves stock without `receiving.District`.

### 6.4 Vanilla fill behavior (no custom WorkplaceBehavior)

Extra targets **reuse** destination `FillInputWorkplaceBehavior` (same as workshops):

1. On `AddExtraTarget`, require `inventory.GetComponent<FillInputWorkplaceBehavior>()`.  
2. Inject that instance into the weighted list with registration weight + prioritize boost.  
3. Patch `CarrierInventoryFinder.TryCarryFromAnyInventoryInternal` so unfinished receivers use **`ConstructionDistrict`** when `District` is null.  
4. `UseExtraHaulerTargets` decorates **`ConstructionSite` → `FillInputWorkplaceBehavior`** so construction sites have the vanilla component.  
5. Fill still uses input goods + `CarryRootBehavior` after reserve.

Weight for sort:

```text
base = registration.Weight
if SetExtraTargetPrioritized(inv, true) && base >= 0.5f
  effective = base + 0.5f
else
  effective = base
```

### 6.5 What we reuse

| Piece | Reuse? |
|-------|--------|
| `FillInputWorkplaceBehavior` | **Yes** (on destination) |
| `CarryRootBehavior` walk + give | **Yes** |
| `GoodReserver` / `CarryAmountCalculator` | **Yes** (via vanilla fill + finder patch) |
| `WeightedBehavior` + district sort | **Yes** (join same list) |
| `HaulCandidate` prioritize formula | **Yes** (`+0.5` when prioritized & weight ≥ 0.5) |
| Finished-only `HaulCandidate` set | **No** for extras |
| `TryCarryFromAnyInventory` district | **Patched** — `ConstructionDistrict` fallback |

### 6.6 Entrances

**Not required.** Destination access is whatever `Accessible` the caller registered (for construction sites: multi-access surrounding + above). Warehouse **sources** still use their normal finished access/entrances.

---

## 7. Service internals

```text
ExtraHaulerTargetService
  - Game thread only
  - Map: Inventory → (Registration, Prioritized, FillWorkplaceBehavior)
  - Index: DistrictCenter → set/list of Inventory keys
  - Add: store registration; prioritized=false; cache fill behavior; index districts
  - Remove/Dispose: unindex; ignore double-remove
  - SetExtraTargetPrioritized: if not in map → return; else set flag
  - GetTargetsForDistrict: registrations for district; purge dead inventories lazily
```

**Dead targets:** if inventory entity is destroyed without `Remove`, drop on next query when component is missing/disabled.

**Weight ordering:** each registration becomes one `WeightedBehavior(effectiveWeight, fillBehavior)` merged **before** the district-wide sort (via the Prefix rewrite of `GetWorkplaceBehaviorsOrdered`).

---

## 8. Construction-site specifics

### 8.1 Inventory

Vanilla construction inventory:

- Goods from `BuildingCost` as **givable** → appear as **InputGoods**  
- Capacity = total cost  
- Enabled while unfinished  

`OnlyInputGoods = true` is correct for the construction example.

### 8.2 Which district to pass

Prefer:

```text
ConstructionDistrict ?? District
```

Haulers must belong to a district that has a **Hauling Center**. If the site’s construction district has no hauling post, registration still works but no hauler will serve it until one exists — same as other haul candidates in spirit.

### 8.3 Scope of the example mod/feature

This plan is the **MT API**. Shipping “all construction sites always accept haulers” can be:

- a thin consumer in MT (optional flag), or  
- a separate small mod that registers every unfinished `ConstructionSite`  

Default MT library behavior: **API only**, no automatic registration of all sites (keeps behavior opt-in and predictable).

---

## 9. Persistence

| Data | Persist? |
|------|----------|
| Registrations in the service | **No** (v1) |
| Caller intent (“this site should be haul-fed”) | Caller’s component / save |

On load: unfinished sites re-enter unfinished → consumer re-calls `AddExtraTarget`.

---

## 10. Edge cases

1. **Site finishes** — must `Remove`; construction inventory disables.  
2. **Site moves district** — remove + re-add with new district list.  
3. **Builders and haulers race** — reservations on unreserved capacity; OK.  
4. **Over-weight extras** — can starve normal hauling; document sensible default weights.  
5. **Empty districts list** — reject or no-op in v1 (no global register).  
6. **Accessible disabled** — skip target until enabled again.  
7. **Marked for emptying** — if destination has `Emptiable` marked, skip (mirror fill-input caution) when present.  
8. **Duplicate Add same Inventory** — replace registration or throw; pick **replace** for simplicity.

---

## 11. Complexity

| Piece | Difficulty |
|-------|------------|
| Registry + Add/Remove/Dispose + Prioritized map | Easy |
| District index | Easy |
| Harmony Prefix reimplement `GetWorkplaceBehaviorsOrdered` | **Easy–Medium** (no Transpiler) |
| Separate workplace behavior / postfix-append | Easy but **wrong** for priority |
| Transpiler inject before Sort | Medium–Hard, avoid |
| Reserve path (workplace district stock → target) | Medium |
| Construction-site consumer example | Easy once API works |

**Overall:** Medium. Harmony is the easy part; reservation path is the real work.

---

## 12. Phased delivery

### Phase 1 — Core API

- [ ] `ExtraHaulerTargetRegistration` as `readonly record struct`  
- [ ] `IExtraHaulerTargetService` + Add/Remove/Dispose  
- [ ] `SetExtraTargetPrioritized` / `IsExtraTargetPrioritized` (no-op if missing)  
- [ ] `UseExtraHaulerTargets()`  
- [ ] Harmony **Prefix** on `DistrictHaulCandidates.GetWorkplaceBehaviorsOrdered` (merge weighted extras, sort, skip original)  
- [ ] Per-inventory fill `WorkplaceBehavior` (workplace-district stock search)  
- [ ] Manual test: register unfinished CS; haulers deliver; prioritize changes order  
- [ ] Dispose/Remove clears service  

### Phase 2 — Hardening

- [ ] Lazy purge of destroyed inventories  
- [ ] Optional helper: `RegisterConstructionSite(ConstructionSite)` convenience  
- [ ] Docs + demo snippet  

### Phase 3 — Optional productization

- [ ] Setting: “Haulers supply construction sites” auto-register all unfinished CS  
- [ ] Priority / per-site disable UI (if ever needed)  

---

## 13. Relation to other plans

| System | Relationship |
|--------|----------------|
| Vanilla construction | Example consumer: register CS inventory + `ConstructionSiteAccessible` |
| Dynamic Construction Site (if built) | Registers DCS inventory the same way |
| Crane (future) | May use normal `HaulCandidate` when finished; or this API for odd hosts |
| Disable builder materials (separate) | Complements this: haulers supply, builders only hammer |

---

## 14. Decisions (locked for this plan)

| Topic | Decision |
|-------|----------|
| Registration type | `readonly record struct ExtraHaulerTargetRegistration` |
| API shape | Concrete `ExtraHaulerTargetService` (no interface) |
| Fill behavior | **Vanilla `FillInputWorkplaceBehavior`** on destination — no custom fill class |
| Stock district patch | **Transpiler**: if inventory is **registered**, stock district = **hauler workplace district**; else vanilla `receiving.District`. Registration `Districts` only gate which posts get the job. |
| ConstructionSite | Decorated with `FillInputWorkplaceBehavior` when feature enabled |
| Prioritize | `SetExtraTargetPrioritized(Inventory, bool)` — **no-op if not registered** |
| Prioritize formula | Same as vanilla: `+0.5` if prioritized and weight ≥ 0.5 |
| Job injection | Harmony **Prefix** rewrite of `DistrictHaulCandidates.GetWorkplaceBehaviorsOrdered` |
| Transpiler | **No** |
| Custom `ExtraHaulFillWorkplaceBehavior` | **Removed** |
| Persistence | Caller re-registers; service is runtime; prioritize flag too unless caller sets again |
| Auto all CS | Not default in core API |
| Context | Game only |

---

## 15. One-sentence summary

**`ExtraHaulerTargetService` lets mods register any inventory (e.g. an unfinished construction site) as a district-scoped haul destination so haulers reserve and deliver like builders already do—without finished `HaulCandidate`s or entrances.**
