# Dynamic Construction Site (DCS) — Design Plan

**DCS is a runtime, builder-workable construction project with optional per-cell occupation markers; it never becomes a permanent building, supports 0-cost/0-time edge cases, refunds like construction by default, and finishing only fires events and cleans reservations so modders can apply buffs or swap real buildings.**

Brainstorming / design doc for a **Dynamic Construction Site** subsystem in **ModdableTimberborn**: a construction job that **builders can work on**, created **at runtime**, that is **not** a normal permanent building. It is a **parallel copy** of the vanilla construction pipeline, **not** a reuse/subclass of `ConstructionSite`.

**Status:** design only (not implemented).  
**Folder:** `ModdableTimberborn/DynamicConstructionSite/`

---

## 1. Motivation

Mods need builders to:

- gather materials, and  
- spend build time  

…without placing a real permanent building, and with optional **space reservation** so a later replace/expand is legal.

---

## 2. Goals

| Goal | Description |
|------|-------------|
| Runtime create | Spawn a construction job from code with cost, priority, time |
| Builder integration | District builders find and work the job (materials + hammer) |
| Not a real building | On complete, site **does nothing by itself** except fire events and clean up |
| Events for modders | `Finished` / `Cancelled` / progress hooks; modder applies buffs, swaps buildings, etc. |
| Optional occupation | Reserve **only the extra** block occupation needed for a future footprint |
| Occupation helpers | Diff / validate / “what gets destroyed” / place & remove 1×1 markers — **same subsystem**, not a separate enabler |
| Opt-in subsystem | `ModdableTimberbornRegistry.Instance.UseDynamicConstructionSites()` |
| Parallel stack | **Copy** vanilla concepts; do **not** subclass or cast to `ConstructionSite` |
| Access | Builders reach DCS like a normal construction site (surrounding cells + above) — **no entrance spawn** |
| Hauler-friendly (stretch) | Prefer designs that let haulers also deliver when possible, including weakly districted / undistricted cases |

## 3. Non-goals (v1)

- Replacing vanilla `ConstructionSite` / builder hub behavior  
- Making haulers deliver **directly** to unfinished **vanilla** construction sites (separate feature)  
- Full crane building content (crane is a **consumer** of DCS later)  
- True multi-tile **building** synthesis (arbitrary NxMxK permanent BO at runtime)  
- Custom `IBlockObjectPlacer` for tools (services place markers)  
- Fancy construction models / slot polish (optional later)  
- Auto-placing the target building on finish (modder decides)  
- Science cost handling (mods subtract/refund science themselves via lifecycle hooks)  
- Map Editor support  
- Overridable occupation markers (intentionally limited)  
- Spawning any entrance / doorstep entity for access  

---

## 4. High-level architecture

One opt-in subsystem. Occupation helpers live **under** DCS (static helpers where pure; DI singletons where they need game services).

```text
┌─────────────────────────────────────────────────────────────┐
│  UseDynamicConstructionSites()                              │
│                                                             │
│  IDynamicConstructionSiteService                            │
│       │                                                     │
│       ├── job / inventory / builder pipeline                │
│       └── occupation services (diff, preview, markers)      │
└─────────────────────────────────────────────────────────────┘
```

```text
Create DCS
  → Plan occupation (optional)
  → Place markers (optional)
  → Register job for builders
  → Materials + work
  → Finish/Cancel
  → Default refund on cancel (unless disabled)
  → Remove markers
  → Raise event
  → Modder does real world change
```

**Context:** **Game only** (not Map Editor).

---

## 5. Vanilla construction (what we mirror)

Vanilla unfinished buildings:

1. `BuildingJobProvider` → `ConstructionRegistry` → `ConstructionJob`  
2. If materials missing: reserve stock + site capacity → `CarryRootBehavior` delivers  
3. If `ReadyToBuild`: `BuildBehavior` / `BuildExecutor` / `Builder` reservation  
4. Inventory full + build time → `MarkAsFinished` as permanent building  

**Important:** material delivery is **not** “Builder carry behavior” — it is reservation + root carry. Actual building **is** typed to `ConstructionSite`.

Therefore DCS needs:

| Vanilla | DCS parallel |
|---------|----------------|
| `ConstructionSite` | `DynamicConstructionSite` (logic + inventory + progress) |
| `ConstructionRegistry` | `DynamicConstructionRegistry` |
| `ConstructionJob` | `DynamicConstructionJob` |
| `BuildingJobProvider` | `DynamicConstructionJobProvider` via `MultiBind<IBuilderJobProvider>` |
| `BuildBehavior` / `Builder` | `DynamicBuildBehavior` + reservation (do **not** call vanilla `BuildBehavior`) |
| `ConstructionRegistrar` | register/unregister on create/destroy |
| `DeleteOnFinish` (tunnels) | always clean up; never leave a finished permanent dummy building |
| Construction access | Surrounding / above access like normal CS — **no entrance** |

Workers already have vanilla `Builder`/`BuildBehavior` for real sites. DCS jobs launch **our** build behavior path for DCS only.

**`IBuilderJobProvider.ProviderPriority`:** keep **default / same band as normal building jobs** (`BuildingJobProvider` uses `1`). Do not special-case vs demolish/recover unless we later need it.

---

## 6. Use cases

### 6.1 Innovation / invisible site (no footprint change)

Example: run a builder project on/near an existing building; on finish apply a buff or unlock.

- Occupation diff **empty** → **no marker BOs**  
- DCS is job + inventory + events only  
- Optional **attached entity** (`EntityComponent?`) for ownership / save binding  
- On finish: fire event → delete DCS → modder applies effect  

### 6.2 Replace / expand footprint

Example: **Platform (1×1×1) → Double Platform (1×1×2)**.

Vanilla block data:

| Local cell | Platform | Double Platform | Extra occupation for DCS |
|------------|----------|-----------------|---------------------------|
| z0 | `Top \| Corners` | `Corners` | **None** (already covered) |
| z1 | — | `Top \| Corners` | **`Top \| Corners`** |

Flow:

1. Compute occupation **delta** (target needs that source does not provide)  
2. Validate reservation / show blockers / optional destroy list  
3. Place **1×1 marker BOs** only for extra cells/flags  
4. Run DCS (materials + builders)  
5. On finish: remove markers → modder deletes Platform, places Double Platform  

### 6.3 Future: Crane (consumer, not in v1)

- Crane = finished building, public inventory, haulers fill it  
- Local workers or builders feed DCS / nearby sites from crane stock  
- Height tiers + 100% refund = separate building design  
- DCS remains the “project” API under the crane  

---

## 7. Occupation system (delta, not bounding box)

### 7.1 Principle

Occupation is **not** “spawn a solid XxYxZ box.”

It is a **per-world-cell occupation delta**:

```text
extra = target.Occupation[cell] & ~source.Occupation[cell]
if extra != None → need a marker at that cell with exactly those flags
```

- No footprint change → empty delta → no markers  
- Expand/replace → only **new** flags/cells  

Also surface (even if not “occupation”) changes in **Stackable**, **MatterBelow**, **OccupyAllBelow** when planning final placement (Platform→Double changes stackable at z0).

### 7.2 Public types (occupation)

```csharp
public readonly record struct OccupationDeltaCell(
    Vector3Int Coordinates,           // world
    BlockOccupations ExtraOccupation,
    BlockSpec? SourceBlock,
    BlockSpec? TargetBlock
);

public sealed class OccupationDiff
{
    public IReadOnlyList<OccupationDeltaCell> Cells { get; }
    public bool IsEmpty => Cells.Count == 0;
    // optional: MatterBelow/Stackable change reports
}

public sealed class OccupationValidationResult
{
    public bool CanReserve { get; }
    public IReadOnlyList<OccupationConflict> Conflicts { get; }
}

public sealed class OccupationPlan
{
    public OccupationDiff Diff { get; }
    public OccupationValidationResult Reservation { get; }
    public DestroyingEntities BlockingForFinalPlacement { get; }
    public bool CanStartWithoutDestruction { get; }
    public bool CanStartIfDestroyBlocking { get; }
}
```

### 7.3 Services / helpers (occupation)

Occupation APIs ship **with** DCS (not a separate `Use…` enabler). Pure geometry can be **static** helpers; anything needing `IBlockService`, `DestructionService`, etc. is a **singleton**.

```csharp
// Pure / static-friendly
public static class BlockOccupationDiff
{
    public static OccupationDiff Compute(
        BlockObjectSpec source, Placement sourcePlacement,
        BlockObjectSpec target, Placement targetPlacement);

    public static OccupationDiff Compute(
        BlockObjectSpec target, Placement targetPlacement);
}

// DI singletons (need world services)
public interface IDynamicOccupationPreviewService
{
    OccupationValidationResult ValidateReservation(
        OccupationDiff diff,
        IEnumerable<BlockObject>? ignoreObjects = null);

    DestroyingEntities QueryBlockingForTarget(
        BlockObjectSpec target,
        Placement targetPlacement,
        IEnumerable<BlockObject>? ignoreObjects = null);

    OccupationPlan Plan(
        BlockObjectSpec? source, Placement? sourcePlacement,
        BlockObjectSpec target, Placement targetPlacement);
}

public interface IDynamicOccupationMarkerService
{
    IReadOnlyList<BlockObject> PlaceMarkers(
        OccupationDiff diff,
        Guid? siteId = null);

    void RemoveMarkers(IEnumerable<BlockObject> markers);
    void RemoveMarkersForSite(Guid siteId);
}
```

**Implementation notes:**

- Diff: `BlockObjectSpec.GetBlocks(placement)`, merge by world coordinates, `extra = target & ~source`  
- Validate: occupation intersects via `IBlockService` / `BlockValidator` patterns; **ignore** source building and same-plan markers  
- Blocking: reuse `DestructionService.QueryDestructingEntities`  
- **No `IBlockObjectPlacer`** — place via `EntityService` / `BlockObjectFactory`  

### 7.4 Runtime 1×1 marker block objects

Markers are **reservation volumes**, not buildings.

Per distinct `BlockOccupations` mask, cache a **Blueprint** (TemplateInstantiator caches by blueprint reference):

```text
BlockOccupations flags → Blueprint "MT.DcsMarker.{Flags}"
```

Minimal `BlockObjectSpec` per cell:

| Field | Value |
|-------|--------|
| Size | `1,1,1` |
| Occupations | exact `extra` flags |
| MatterBelow | `Any` (do not invent support requirements) |
| Stackable | `None` (do not act as platform) |
| OccupyAllBelow | false |
| Underground | false |
| **Overridable** | **`false`** (intentionally not overridable) |

Plus:

- `TemplateSpec` with stable name  
- Tag/component `DynamicOccupationMarker { Guid SiteId }`  
- **No `BuildingSpec`** (avoid construction/building decorators)  
- **Unfinished** state (aligned with construction-site-like reservation; finished vs unfinished is secondary — pick unfinished and stick to it)  
- Not player-placeable (no tool group / hidden)  
- **No entrance**  
- Nav: avoid surprising floors/walls (`NoAutoWalls`, conservative nav settings)  

On DCS destroy (finish or cancel): **always remove markers**.

### 7.5 Occupation algorithm (reference)

```text
function ComputeDiff(source?, sourcePlacement?, target, targetPlacement):
  map worldCoord → (sourceBlock?, targetBlock?)
  for each block in target.GetBlocks(targetPlacement):
    map[block.Coordinates].target = block
  if source:
    for each block in source.GetBlocks(sourcePlacement):
      map[block.Coordinates].source = block

  for each (coord, src, tgt) in map:
    srcOcc = src?.Occupation ?? None
    tgtOcc = tgt?.Occupation ?? None
    extra = tgtOcc & ~srcOcc
    if extra != None:
      yield OccupationDeltaCell(coord, extra, src, tgt)
```

---

## 8. Dynamic Construction Site (job system)

### 8.1 Cost and time

Mirrors vanilla flexibility:

| Cost | Time | Behavior |
|------|------|----------|
| Non-empty | Derived or explicit hours | Normal materials + work |
| **Empty (0 cost)** | Explicit hours | Work-only (no material delivery) |
| Non-empty | **0 hours** | Materials only; finishes when inventory full |
| **Empty + 0 hours** | — | **Finish immediately** (allowed; usually means the API caller misconfigured — not an error we hard-block) |

- `Cost`: empty list / no goods allowed  
- `ConstructionTimeInHours`: `null` = derive like vanilla material ratio when cost non-empty; `0` = no work phase  
- Do not special-case science; mods handle science outside DCS  

### 8.2 Public API (DCS)

```csharp
public sealed class DynamicConstructionSiteRequest
{
    public Vector3Int Origin { get; init; }
    public IReadOnlyList<GoodAmount> Cost { get; init; } = [];
    public float? ConstructionTimeInHours { get; init; }
    public Priority Priority { get; init; } = Priority.Normal;
    public string? DisplayNameLocKey { get; init; }

    /// Optional owner entity (any entity component). Saved/loaded by entity id when present.
    public EntityComponent? AttachedEntity { get; init; }

    public OccupationDiff? Occupation { get; init; }
    public OccupationPlan? Plan { get; init; }
    public bool PlaceOccupationMarkers { get; init; } = true;

    /// When true, Cancel does not spawn default recoverable refund; modder handles OnCancelled.
    public bool DisableDefaultRefund { get; init; } = false;

    public object? UserData { get; init; } // session-only
}

public interface IDynamicConstructionSite
{
    Guid Id { get; }
    bool IsActive { get; }
    float MaterialProgress { get; }
    float BuildProgress { get; }
    Priority Priority { get; set; }
    Inventory? Inventory { get; }

    EntityComponent? AttachedEntity { get; }
    IReadOnlyList<BlockObject> OccupationMarkers { get; }
    IReadOnlyList<Vector3Int> OccupiedCells { get; }
    object? UserData { get; }
    bool DisableDefaultRefund { get; }

    event Action<DynamicConstructionSiteFinishedEventArgs> Finished;
    event Action<DynamicConstructionSiteCancelledEventArgs> Cancelled;
    event Action Progressed;

    void FinishNow();
    void Cancel();
}

public interface IDynamicConstructionSiteService
{
    IDynamicConstructionSite Create(DynamicConstructionSiteRequest request);
    bool TryGet(Guid id, out IDynamicConstructionSite site);
    IReadOnlyCollection<IDynamicConstructionSite> ActiveSites { get; }
}
```

**Attachment:** optional `EntityComponent?` (not only `BlockObject`). Not compulsory. Persist by **entity id** on save/load when set. Useful for “this DCS belongs to that building/mod component” without requiring a building-shaped anchor.

### 8.3 Internal stack (copy, not reuse)

```text
DynamicConstructionSite/
  Plan.MD
  Public/           interfaces, request, events
  Occupation/       diff (static), preview/markers (singleton)
  Internal/
    DynamicConstructionSite.cs
    DynamicConstructionRegistry.cs
    DynamicConstructionJob.cs
    DynamicConstructionJobProvider.cs   // MultiBind IBuilderJobProvider, default priority
    DynamicBuildBehavior.cs
    DynamicBuilderReservation.cs
    DynamicConstructionSiteFactory.cs
  UI/               optional later
  README.md
```

**Builder hub integration:**

```csharp
MultiBind<IBuilderJobProvider>().To<DynamicConstructionJobProvider>().AsSingleton();
// ProviderPriority: default / same as BuildingJobProvider (1)
```

**Job decide (mirror `ConstructionJob`):**

1. Path/access from builder workplace to DCS access (surrounding + above, no entrance)  
2. If ready to build → start `DynamicBuildBehavior`  
3. Else if cost remaining: find stock, reserve stock + DCS inventory capacity → `ReleaseNextTick` → `CarryRootBehavior`  
4. Never call vanilla `BuildBehavior` / never reserve vanilla `ConstructionSite`  

**Finish policy:**

- When materials + work complete (or immediate if 0+0): fire `Finished`, remove markers, unregister job, **delete DCS host**  
- Do **not** `MarkAsFinished` into a permanent dummy building  

**Cancel / refund policy:**

| Mode | Behavior |
|------|----------|
| Default (`DisableDefaultRefund == false`) | **Full refund** of materials still on the site (and un-deliver reserved goods as appropriate), as **recoverable good stack** — same spirit as cancelling / recovering construction materials |
| `DisableDefaultRefund == true` | No default stack; raise `Cancelled` and let modder decide (e.g. zero refund) |

In-progress carriers: unreserve / abort safely (implementation detail; prefer not duplicating goods).

### 8.4 Host entity

Public API does not force a particular host shape. Internally:

- Hidden DCS host for inventory + job + accessible construction-site-like access  
- **0..N occupation markers** (unfinished 1×1, non-overridable)  
- Optional **attached entity** reference only for ownership/lifecycle — **not** required for access  

Do **not** spawn entrance entities. Do **not** require `AttachedEntity` for builders to path.

### 8.5 Access / pathing

**Builders (required):**

- Access like a **normal construction site**: all surrounding cells and above (vanilla `ConstructionSiteAccessible`-style range — not a single door entrance)  
- **Never** spawn an entrance  
- Path from builder workplace using district construction/road rules as applicable  

**Haulers (desired / stretch):**

- If we can allow haulers to deliver into DCS inventory (including cases that are poorly districted / undistricted), that is **great**  
- May need public input inventory and/or haul candidate wiring carefully so it does not force finished-building-only haul rules  
- v1 success criterion is **builders**; hauler support is **best-effort** when it does not explode scope  

**District:** unfinished-style host likely participates in construction-district assignment. Prefer pathability from the builders’ hut road network.

---

## 9. Modder usage sketches

### 9.1 Buff / innovation (no occupation)

```csharp
var site = dcs.Create(new DynamicConstructionSiteRequest
{
    Origin = building.Coordinates,
    AttachedEntity = building.GetComponentFast<EntityComponent>(),
    Cost = [new("Plank", 20), new("Gear", 5)],
    PlaceOccupationMarkers = false,
    UserData = building,
});

site.Finished += _ =>
{
    // apply buff, unlock, etc.
};
```

### 9.2 Platform → Double Platform

```csharp
var plan = occupationPreview.Plan(
    platformSpec, platformPlacement,
    doublePlatformSpec, doublePlatformPlacement);

if (!plan.CanStartWithoutDestruction && !plan.CanStartIfDestroyBlocking)
{
    // show "can't upgrade"
    return;
}

// optional: show plan.BlockingForFinalPlacement to player, then destroy

var site = dcs.Create(new DynamicConstructionSiteRequest
{
    Origin = platformPlacement.Coordinates,
    AttachedEntity = platformBo.GetComponentFast<EntityComponent>(),
    Plan = plan,
    Cost = upgradeCost,
    PlaceOccupationMarkers = true,
});

site.Finished += _ =>
{
    entityService.Delete(platformBo);
    spawning.PlaceObject(doublePlatformSpec, doublePlatformPlacement);
};
```

### 9.3 Cancel with no refund

```csharp
var site = dcs.Create(new DynamicConstructionSiteRequest
{
    Origin = origin,
    Cost = materials,
    DisableDefaultRefund = true,
});

site.Cancelled += args =>
{
    // keep materials destroyed / partial refund / custom logic
};
```

### 9.4 Immediate finish (allowed but usually a caller bug)

```csharp
dcs.Create(new DynamicConstructionSiteRequest
{
    Origin = origin,
    Cost = [],
    ConstructionTimeInHours = 0f,
});
// finishes immediately → Finished fires
```

Helpers alone (still under DCS subsystem):

```csharp
var diff = BlockOccupationDiff.Compute(srcSpec, srcPlace, dstSpec, dstPlace);
var validation = preview.ValidateReservation(diff, ignoreObjects: [sourceBo]);
var blocking = preview.QueryBlockingForTarget(dstSpec, dstPlace, ignoreObjects: [sourceBo]);
```

---

## 10. Persistence

| Data | Persist? | How |
|------|----------|-----|
| DCS id, progress, cost remaining, priority | Yes | site host / registry |
| Occupation markers | Yes | BO save + `SiteId` on marker component |
| `AttachedEntity` | Yes | entity id when set; clear if missing on load |
| `DisableDefaultRefund` | Yes | flag on site |
| `UserData` | **No** | session only |
| Finish/cancel handlers | **Not as delegates** | re-bind on load via attached owner |

**Recommended owner pattern:** component on attached entity stores DCS id + upgrade payload; on load re-subscribes to `Finished`/`Cancelled`.

Science: not persisted by DCS; mod own state if needed.

---

## 11. UI (optional / later)

v1 can be headless (code-created only).

Later:

- Fragment: materials, progress, priority, cancel  
- Priority integration with builder priority tools if desired  
- Preview highlight for occupation plan cells / blocking objects  

---

## 12. Enabling the subsystem

```csharp
ModdableTimberbornRegistry.Instance
    .UseDynamicConstructionSites();
```

- Single enabler (occupation helpers included)  
- Chainable, idempotent  
- **Game context only**  
- Document in `ModdableTimberborn/Docs/` when implemented  

---

## 13. Phased delivery

### Phase 0 — Occupation APIs (under DCS folder)

- [ ] `BlockOccupationDiff` (static)  
- [ ] `IDynamicOccupationPreviewService`  
- [ ] Marker blueprint cache by `BlockOccupations`  
- [ ] `IDynamicOccupationMarkerService` place/remove (unfinished, non-overridable)  
- [ ] Tests: Platform→DoublePlatform delta  
- [ ] No builder job yet  

### Phase 1 — DCS MVP

- [ ] Site inventory + progress (0 cost / 0 hours / both allowed)  
- [ ] Registry + `IBuilderJobProvider` (default priority)  
- [ ] Material path + `CarryRootBehavior`  
- [ ] `DynamicBuildBehavior`  
- [ ] Access: surrounding + above, **no entrance**  
- [ ] Create / FinishNow / Cancel  
- [ ] Default full refund stack; `DisableDefaultRefund`  
- [ ] Events; remove markers; delete site  
- [ ] Save/load progress + attached entity id  
- [ ] Optional `AttachedEntity`  

### Phase 2 — Expand/replace ergonomics + hauler stretch

- [ ] `OccupationPlan` on create polished  
- [ ] Ignore-sets for source + markers  
- [ ] Preview highlights  
- [ ] Owner/attached finish-handler demo  
- [ ] Investigate hauler delivery to DCS inventory (undistricted / construction-district cases)  

### Phase 3 — Polish / consumers

- [ ] Optional UI fragment  
- [ ] Crane mod as consumer  
- [ ] Optional “builders don’t fetch materials” companion  

---

## 14. Difficulty snapshot

| Piece | Difficulty | Notes |
|-------|------------|--------|
| Occupation diff | Easy–Medium | Pure block geometry |
| Validate + blocking | Medium | BlockValidator + DestructionService |
| Runtime 1×1 markers | Medium | Cached blueprints; unfinished; non-overridable |
| Custom Placer | **Skip** | Services place directly |
| DCS materials + build | Hard | Parallel construction stack |
| CS-like access without entrance | Medium | Mirror `ConstructionSiteAccessible` style |
| Hauler delivery | Medium–Hard | Stretch; finished-haul rules may fight us |
| Finish/cancel + refund | Medium | Recoverable stack like CS |
| Persistence of effects | Medium | Attached entity + re-bind handlers |

---

## 15. Risks and edge cases

1. **Mid-job cancel** — unreserve stock/capacity; agents mid-carry; default refund must not duplicate goods.  
2. **Self-conflict** — validators ignore attached source BO and this site’s markers.  
3. **Navmesh noise** — markers must not create weird path floors.  
4. **District / access** — builders must reach surrounding/above access without an entrance.  
5. **Save template names** — marker blueprint names stable across versions.  
6. **Physics after swap** — modder order when deleting source / placing target.  
7. **Double-finish** — idempotent finish/cancel.  
8. **Priority changes** — registry re-bucket like `ConstructionRegistrar`.  
9. **Attached entity deleted first** — cancel or orphan policy (prefer cancel site).  
10. **0 cost + 0 hours** — immediate finish; document as usually a caller mistake.  

---

## 16. Relation to other systems in this repo

| Existing | Relationship |
|----------|----------------|
| `BlockObjectSpawningHelper` | Place final target buildings after finish |
| `DestructionService` | Blocking preview / optional clear for expand |
| BuildingRenovations | Materials + timer **without** builders; DCS is builder-driven alternative |
| `DisableHauling` pattern | Model for optional later flags |
| Bonus System | Typical `Finished` consumer for innovation buffs |
| Crane (future mod) | Consumer of DCS + public inventory / haulers |

---

## 17. Decisions (locked)

| Topic | Decision |
|-------|----------|
| Reuse `ConstructionSite`? | **No** — parallel copy |
| Placer for markers? | **Skip** |
| Occupation model | **Per-cell delta** (`target & ~source`) |
| Markers | Runtime **1×1**, specs on the fly, **cached by occupation flags** |
| Marker state | **Unfinished** |
| Marker overridable? | **No** |
| On DCS destroyed | Remove markers; modder decides transform |
| Innovation | Empty delta → no markers |
| 0 cost | **Allowed** (work-only) |
| 0 hours | **Allowed** (materials-only or instant with 0 cost) |
| 0 cost + 0 hours | **Immediate finish** (allowed; usually caller misconfig) |
| Attach | Optional **`EntityComponent?`**, save/load by entity id; not required |
| Access | Surrounding + above like normal CS; **never spawn entrance** |
| Haulers | **Best-effort / stretch** — great if undistricted delivery works |
| Job provider priority | **Default** (same as normal buildings) |
| Cancel refund | **Default full refund as recoverable stack** (like CS) |
| Custom cancel refund | `DisableDefaultRefund = true` + `Cancelled` hook |
| Occupation enabler | **No separate Use…** — part of DCS subsystem |
| Occupation helpers | Static where pure; singleton when needing DI |
| Context | **Game only** |
| Science | **Out of scope** (mods handle via lifecycle) |
| Map Editor | **No** |
| Crane | Later consumer |

---

## 18. Residual implementation notes (not open product questions)

These are engineering choices left to implementers, not design debates:

- Exact DCS host prefab/blueprint composition (inventory + accessible)  
- How aggressive hauler support is in Phase 1 vs Phase 2  
- Whether `BlockOccupationDiff` is a static class name vs small non-DI service  
- Precise recoverable-stack API to call for default refund  

---

## 19. Suggested first implementation spike

1. Diff Platform→DoublePlatform in a test; assert single cell `Top|Corners` at +Z.  
2. Place one **unfinished, non-overridable** marker BO from a runtime blueprint; confirm occupation blocks.  
3. Delete marker; confirm cell free.  
4. Stub `IBuilderJobProvider` that logs job discovery.  
5. Only then full material + build path.
