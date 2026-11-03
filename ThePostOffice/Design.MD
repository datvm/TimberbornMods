# The Post Office

Folktails only. There is no save migration. `BackwardCompatibleTemplateNames` stays empty.

The mod uses Global Inventory through `Targets/CommonGlobalInventory.targets`.

## Buildings

Three buildings share one tool group, `PostOffice`, parented under District Management. The group icon is the Dispatch Post icon.

- **Dispatch Post** (`DispatchPost.Folktails`) sends goods from this district into shared storage. It is a workplace. `PostOfficeSenderSpec.SendHours` is 3. It contributes 120 kg.
- **Parcel Office** (`ParcelOffice.Folktails`) brings goods out of shared storage for this district. It is a workplace. `PostOfficeReceiverSpec.RetrieveHours` is 3. `InventoryCapacity` is 80 kg. It contributes 120 kg.
- **Holding Lodge** (`HoldingLodge.Folktails`) only adds shared storage. It contributes 600 kg. It has no inventory and no workplace.

Both workplaces also have an empty `PostOfficeLocalInventorySpec`. Each finished, district-connected building adds its `PostOfficeCapacitySpec.Contribution` to the shared weight limit. Losing that building removes the contribution.

## Terms

- **Parcel.** One pack of goods that one job sends or retrieves. The goods in a parcel can differ.
- **Manual parcel.** A parcel the player ordered. Manuals can be removed.
- **Auto parcel.** At most one, always queued after the manuals, with no remove button. A new manual does not jump ahead of the parcel that is already active.
- **Shared weight limit.** The maximum stored weight of the global inventory.
- **Incoming hold.** Unsaved weight reserved for a Dispatch Post job.
- **Outgoing hold.** Unsaved amounts of specific goods reserved for a Parcel Office job.

`PostOfficeConfigSpec.ParcelWeight` is 60 kg. The player names goods and amounts. Those amounts are split, in list order, into parcels. A single unit heavier than 60 kg is its own parcel. A light parcel takes the same time as a full one. Worker speed shortens the time. The worker works in 0.25 h chunks.

## Reservations

Reservations live only in `PostOfficeGlobalInventoryService`. They are not saved.

- Remaining shared weight is capacity minus stored weight minus incoming holds.
- The available amount of a good is the stored amount minus outgoing holds.
- Each hold has an id. `TryReserve` creates one, `Release` drops it, and `Commit` finishes it.
- Committing an incoming hold adds the saved goods to global storage. Committing an outgoing hold removes them.

Losing a building still spills stored global goods. That path also drops holds that no longer fit, newest first, and posts an event. The affected job cancels. A Dispatch Post job that already took its goods out of the building drops them as a recovered stack.

On load, a saved job asks for its hold again. If it cannot get one, it cancels. A sender that already took goods out of the building spawns those goods with `RecoveredGoodStackSpawner.AddAwaitingGoods` and does not put them back.

## Local inventory

`PostOfficeInventory` is attached by `PostOfficeLocalInventorySpec`. The vanilla inventory capacity stays unlimited, so a strange overflow can sit there. The component decides what may enter.

- Dispatch Post accepts only the parcel currently being gathered. That is the active parcel before the job starts, or the next parcel after the job has taken the active goods out. Goods already in the building stay accepted, so they are not treated as unwanted. There is no output, so the gathering goods stay wanted. There is no building weight property. Under normal play the building holds at most one parcel.
- Parcel Office accepts nothing. It gives stock to the district. The allowed amount is the auto keep for each good, or 0 when auto is off or the good is not in the keep list. Stock above that is unwanted. Haulers carry the kept goods to buildings that need them, and they empty the surplus. When the district is full, the keep can sit in the office.

A readonly fragment shows the current weight and the goods inside. The receiver line includes its capacity. The sender line is just the current weight. The global-storage fragment stays separate from both workplace panels.

## Workers and haulers

`WorkerRootBehavior` tries workplace behaviors in decorator order and keeps the first one that does not release. Neither building needs a "needs haulers" flag.

- Dispatch Post: the send job, then `FillInputWorkplaceBehavior`. `FillInputHaulBehaviorProvider` lets district haulers fill the same input. The worker carries goods only while the send job is not runnable.
- Parcel Office: the retrieve job, then `RemoveUnwantedStockWorkplaceBehavior`. The worker carries surplus out only while the retrieve job is not runnable. Haulers do that too, and they also carry kept goods to buildings that need them.

The sender does not remove unwanted stock. The gathering parcel must stay. The receiver does not fill input. Both workplaces stay `Emptiable`.

## Dispatch Post

1. The worker or district haulers fill the active parcel.
2. When that parcel is complete, the office reserves shared weight. While the weight does not fit, the job waits.
3. The job starts. The first work tick takes the goods out of the building and stores the amounts on the component. The timer keeps running.
4. The worker or haulers may fill the next parcel while the timer runs.
5. When the timer ends, the hold is committed into global storage.

`SendHours` stays 3.

Manual parcels can be cancelled. Cancelling the active parcel, or the next one while it is being gathered, drops goods that are still in the building, or goods already held on the component, as a recovered stack. Later queued manuals have no goods yet.

Auto is a reorder point. One auto parcel at a time, always last.

- Tracked amount is global stock plus this office's outstanding sends of that good.
- While auto is on and no auto parcel is queued, if any rule has tracked amount below `Target - Margin`, the office packs one parcel toward `Target`, in rule order, capped at one parcel.
- If tracked amount reaches `Target` before that auto job has taken its goods out, the parcel is removed and any goods already gathered are dropped. A job that has already taken its goods out is allowed to finish.
- Turning auto off, or confirming new rules, uses that same removal.

## Parcel Office

The active parcel reserves those global goods as soon as it is active, and retries when global stock changes. The worker starts only after that hold exists and the building can accept the result.

- The job waits while stock weight plus parcel weight is over `InventoryCapacity` and the parcel could fit in an empty building. Workers and haulers take goods out first.
- If the parcel is heavier than the building even when empty, including one item heavier than the parcel limit, the job starts anyway. The extra weight sits until it is carried out.
- The timer then runs. At the end, the hold is committed and the goods are put in the building. Nothing is removed from global storage at the start.
- Cancel releases the hold. Goods already delivered stay in the building. Surplus is unwanted. There is nothing to turn into a recovered stack.

`RetrieveHours` is 3. `InventoryCapacity` is 80 kg.

Receiver auto is a keep list, not a reorder point. There is no margin.

- The player picks goods and amounts that should sit in this building. The dialog refuses a list whose weight is over `InventoryCapacity`, except one item heavier than that cap, which is still allowed.
- One auto parcel at a time, always last, with no remove button. Its size is the shortage: keep amount minus the amount already in the building and already outstanding. That shortage is packed in list order, capped at one parcel.
- Example: keep 10 Logs and 10 Planks. Haulers carry them to the district, the stock falls, and the next auto parcel replaces what left. When the district stops taking them, 10 Logs and 10 Planks remain and auto stops. If 9 and 9 are inside, the parcel is 1 Log and 1 Plank when that fits in one parcel.
- Turning auto off, or confirming a new list, drops the queued auto parcel. Goods already in the building stay. With auto off, the allowed amount is 0, so haulers empty the building.

## UI

The sender and the receiver each have their own fragment. There is no shared desk panel.

The manual-order dialog is shared. It edits a list of goods and amounts, shows total weight and parcel count, and confirms. Both fragments open it.

The auto dialogs are separate. The sender dialog edits a target and a margin. The receiver dialog edits a keep list and shows that list's weight against `InventoryCapacity`.

Both sidebars use Timberborn controls: Add manual order, Auto order, Configure auto order, the active parcel with a progress bar, then the waiting parcels. A short status line covers the wait reason. Dropdown, integer amount, Add, Confirm, Remove, and Cancel are the controls.

- Sender status: waiting for goods, or waiting for shared weight.
- Receiver status: waiting for global goods, or waiting for room.
