# Crane

WIP Timberborn building: a 1×1 **Workshop** (not a Manufactory) whose worker stays inside and remotely builds / demolishes nearby sites. Height comes from stacked **Crane Sections**. Materials arrive by hauler into the Crane, or the worker fetches them if haulers are busy; `CraneInventory` immediately dumps them onto jobs. The worker only hammers.

Requires [ModdableTimberborn](https://steamcommunity.com/workshop/filedetails/?id=3284904751). Game version 1.1.1.

This is not a finished mod. Height UI, exclusive builder lockout, and range highlight are still missing.

## Intended gameplay

- Place a **Crane** from District Management (next to Builders Hut). 8 Log, 4 Plank, 4 Gear, 300 science. One beaver or bot (bot unlock 500 science).
- The worker walks in and stays. They do not path to construction sites.
- Reach is a square around the mast, unlimited down, up to **one layer above** finished tower height (vanilla builders can hammer a site from one Z below it). Current code uses Chebyshev range **1** (3×3 including the mast). The original plan was 9×9.
- **Crane Sections** (4 Plank + 1 Gear) extend the mast. They are a Dev Mode tool today; the Crane UI is supposed to spawn them unfinished so the tile is occupied immediately and the worker hammers them from inside (they sit on the column, so they are not in geometric reach).
- Builders are supposed to leave claimed work alone. That lockout is not implemented in this pass.

## What exists

| Piece | Status |
|---|---|
| Folktails + Iron Teeth Crane / Section models and blueprints | Done |
| Stacked tower + working bounds | Done |
| Job adapters on every construction site and demolishable | Done |
| Per-crane job set (`Tower.Jobs`) | Done — rebuilds on tower change |
| Worker executor (walk in, hammer / demolish) | Done |
| Crane inventory dumps deliveries onto jobs | Done |
| Recover rubble (`RecoveredGoodStack`) as a crane job (60 items/hour) | Done |
| Height UI (place / remove sections) | Missing |
| Range highlight (`IBuildingWithRange`) | Missing |
| Work-order / Prioritize UI | Done — inventory + job queue fragments |
| Exclusive claim so builders skip crane work | Missing (Harmony patches removed) |
| 100% refund on finished sections | Not actually working (see below) |

## Architecture

Jobs are discovered, not claimed exclusively. Overlap is allowed: the same site can sit in more than one crane’s set. A later picker / executor decides who works it.

```
ICraneJobProvider  ──events──►  CraneClaimService  ──adds/removes──►  CraneTower.Jobs
        ▲                                                                  │
        │ GetJobForCrane (rebuild)                                         ▼
ConstructionSiteCraneJob / DemolishableCraneJob                 CraneInventory
        (IMaterialCraneJob on construction)                     dump deliveries onto jobs
                                                                CraneWorkshop pick → ProgressJob
```

`WorkshopSpec` only adds `Workshop` (who is currently working) plus a productivity counter. It does **not** add inventory, hauling, or worker AI.

- **`CraneInventory`** is not part of work. Haulers / the worker deliver into the crane. On `InventoryChanged`, it scans `Tower.Jobs` in list order and `AddMaterial`s as much as each `IMaterialCraneJob` will take. After a scan the inventory should be empty. If two cranes and a builder fill the same site at once, remainder goes back into the crane; `IGoodDisallower` then drops that good to 0 so it is **unwanted stock**. Haulers and the worker already empty unwanted stock (`RemoveUnwantedStockWorkplaceBehavior`). No `RecoveredGoodStack` eject.
- Allowed goods: every good, capacity 10_000. Real cap is remaining need of this crane’s `IMaterialCraneJob`s.
- `Emptiable` pulls in `HaulCandidate`, so haulers fill via `FillInputHaulBehaviorProvider` / `FillInputWorkplaceBehavior`. The assigned worker uses that same fill behavior when they cannot hammer.
- **`CraneWorkshop`** only hammers / demolishes. `CraneWorkWorkplaceBehavior` walks inside and runs `CraneWorkExecutor`. Speed is `delta * worker.WorkingSpeedMultiplier * 1.33`.
- Worker behavior order: work → fill input → dump unwanted → empty if marked → idle inside.

### Buildings

- **`CraneComponent`** — workplace on the mast. Owns a `CraneTower`. `IsWorking` is “finished and has an assigned worker”. Pause or mark-for-demolish makes the crane ineligible for jobs.
- **`CraneWorkshop`** — job picker and hammer. Not a Manufactory.
- **`CraneInventory`** — haul target; dumps incoming goods onto jobs; leftover is unwanted stock.
- **`CraneSectionComponent`** — one finished level of the mast. `IRecoverableGoodMultiplier` returns `1f` (meant to force full refunds; see gaps). Selecting a section shows **Select Crane**.
- **`CraneFlagIconComponent`** — stamps `CraneFlagIcon.png` onto the pile’s UV2 banner.

### Tower

`CraneStructureService` walks **up** from a finished crane, one tile at a time, attaching finished `CraneSectionComponent`s until a gap or unfinished section.

Working bounds (in `UpdateWorkingBounds`):

- XY: `TowerRange` (currently 1) around the mast → 3×3
- Z: `0` through one layer above the top finished section. Vanilla `ConstructionSiteAccessible` allows builders from `baseZ - 1`, so a beaver at the mast can hammer the tile above it.

Owned **unfinished** sections are not part of the tower and are not in those bounds. `ConstructionSiteCraneJob.IsForCrane` special-cases them: they belong to the owning crane via `FindCraneOfSection` (walk **down** the column). Demolish jobs refuse every section, so the demolish queue cannot eat the mast.

Reach tests use AABB overlap of `BlockObject.GetConstructionBounds()` against `WorkingBounds`, not occupied-cell Chebyshev. That is slightly looser than “any occupied cell in range”.

### Jobs

`ICraneJob` is a stable component on the target entity:

- `IsForCrane` — reach / ownership
- `IsAvailable` — construction: `ConstructionSite.IsOn` (validators + unblocked); demolish: marked and `CanDelete()`
- `ProgressJob(crane, hours)` — `IncreaseBuildTime` / `ProgressDemolition` / pull unreserved rubble into that crane’s inventory

`IMaterialCraneJob` (construction only):

- `GetMaterials` — remaining need (`UnreservedCapacity`)
- `AddMaterial` — `Inventory.GiveExisting` onto the site; returns the amount actually added
- Hammer only when `ConstructionSite.HasMaterialsToResumeBuilding` (same 10% staging gate as builders)

`Tower.Jobs` is an ordered list (not a set). Sort: owned unfinished sections → higher `Priority` → build before demolish. `ICraneJob.PriorityChanged` re-inserts that job. The worker walks the list and takes the first job it can progress.

Providers (`ConstructionCraneJobProvider`, `DemolishCraneJobProvider`, `RecoveredGoodStackCraneJobProvider`) are `MultiBind` of `ICraneJobProvider`. They:

- raise `OnPossibleNewJob` / `OnJobRemoved` when a site enters unfinished / is marked / rubble spawns, or leaves
- can rebuild with `GetJobForCrane` by walking vanilla `ConstructionRegistry` / `DemolishJobs` / `PrioritizedRecoveredGoodStackRegistry`

Rubble jobs only run while a worker is hammering. They move **unreserved** stock into `CraneInventory` at `RecoveredGoodStackCraneJob.ItemsPerHour` (60). Reserved goods stay for a beaver already walking in.

`CraneClaimService` (name is leftover from exclusive claims) keeps each eligible crane’s ordered `Jobs` list. Eligible = finished, unpaused, not marked for demolish. It rebuilds on `OnCraneTowerChanged` and pushes the list into `CraneInventory.OnJobsChanged` so limits and dumps stay in sync.

## File map

```
Components/   Crane, inventory, workshop, work behavior/executor, section, flag icon, job adapters
Jobs/         ICraneJob, IMaterialCraneJob, construction / demolish / rubble providers
Models/       CraneTower
Services/     structure, job listing, inventory initializer, template module
Helpers/      BoundsInt overlap + construction AABB
UI/           section → select crane, inventory, job queue
Root/         Folktails / Iron Teeth blueprints and timbermesh
```

## Remaining work (rough order)

1. **Height UI** — spawn unfinished sections with `ConstructionFactory.CreateAsUnfinished` (not `PlaceObject`, which finishes them). Delete from the top. Hide the Dev Mode tool once that exists.
2. **Builder lockout** (if still wanted) — exclusive claim or Harmony skip on `ConstructionJob.StartConstructionJob` / `DemolishJob.CanStartJob` / ConstructionSiteHauler.
3. **Range highlight** via `IBuildingWithRange`.
4. **Section refunds** — `IRecoverableGoodMultiplier` values are **multiplied**. Returning `1f` does not override `ConstructionSiteRecoverableGoodMultiplier`’s demolish rate on finished construction inventory. Needs a replace (Harmony prefix) for 100% section refunds.

## Known gaps

- **`Tower.Contains` (occupied cells) is unused.** Matching uses AABB `Overlaps`.
- **Overlapping cranes both list the same job** and both will haul its remaining need. No exclusive claim yet.
- **Demolish via `EntityService.Delete` is the vanilla demolisher path** (`Demolisher.Demolish` does the same). Refund piles still come from `Deconstructible` + `BuildingDeconstructedEvent`.

## Compatibility (planned, not wired)

| Mod | Note |
|---|---|
| ConstructionSiteHauler | Must not fill sites a crane is meant to own, once claims exist |
| ExtendedBuilderReach | Independent of crane XY/Z reach |
| InstantBuild | Still goes through `IncreaseBuildTime` |
| ConfigurableWorkplace | Keep max workers at 1 |
