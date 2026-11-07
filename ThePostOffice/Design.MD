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
- **Auto parcel.** A parcel the desk queued from its rules. It has no remove button while auto is on. The active auto can be cancelled only after auto is turned off. A queued auto is removed by changing the rules, not by the remove button.
- **Shared weight limit.** The maximum stored weight of the global inventory.
- **Incoming hold.** Unsaved weight reserved for a Dispatch Post job.
- **Outgoing hold.** Unsaved amounts of specific goods reserved for a Parcel Office job.

A new manual is inserted before the first auto parcel. It never inserts in front of the parcel that is already active. When the active parcel is itself auto, the manual lands immediately after it, in front of a Dispatch Post lookahead.

`PostOfficeConfigSpec.ParcelWeight` is 60 kg. The player names goods and amounts. Those amounts are split, in list order, into parcels. A single unit heavier than 60 kg is its own parcel. A light parcel takes the same time as a full one. Worker speed shortens the time. The worker works in 0.25 h chunks.

Both desks store rules as `PostOfficeAutoRule`: a good, a target, and a margin. A rule with target 0, a negative margin, or an unknown or zero-weight good is dropped. No rules left forces auto off.

## Reservations

Reservations live only in `PostOfficeGlobalInventoryService`. They are not saved.

- Remaining shared weight is capacity minus stored weight minus incoming holds.
- The available amount of a good is the stored amount minus outgoing holds.
- Each hold has an id. `TryReserve` creates one, `Release` drops it, and `Commit` finishes it.
- Committing an incoming hold adds the saved goods to global storage and then drops any other holds that no longer fit. Committing an outgoing hold removes the goods and does the same check.
- That commit path posts `OnPostOfficeHoldsStalled`. The desk forgets its hold id and asks again. It does not spill goods and it does not remove the parcel.

Losing a building still spills stored global goods. That path also drops holds that no longer fit, newest first, and posts `OnPostOfficeHoldsReleased`. The service has already released those holds. The affected job cancels. A Dispatch Post job that already took its goods out of the building drops them as a recovered stack. A job that had only gathered them drops those goods from the building. A Parcel Office job drops the active parcel and resets its timer. Delivered goods stay in the building.

On load, a saved job asks for its hold again. If it cannot get one, it cancels. A sender that already took goods out of the building spawns those goods with `RecoveredGoodStackSpawner.AddAwaitingGoods` and does not put them back. A sender whose parcel is no longer complete in the building drops what is there and cancels, without asking for the hold.

## Local inventory

`PostOfficeInventory` is attached by `PostOfficeLocalInventorySpec`. The vanilla inventory capacity stays unlimited, so a strange overflow can sit there. The component decides what may enter.

- Dispatch Post accepts only the parcel currently being gathered. That is the active parcel before the job starts, or the next parcel after the job has taken the active goods out. Goods already in the building stay accepted, so they are not treated as unwanted. There is no output, so the gathering goods stay wanted. There is no building weight property. Under normal play the building holds at most one parcel.
- Parcel Office accepts nothing. Its goods are output. `CopyAccepted` adds nothing, so the allowed amount stays 0 and the whole stock is unwanted. The keep list does not mark a pile as wanted. Haulers empty the building. The worker does that too, and only while the retrieve job is not runnable. Goods that are still inside still count toward the keep.

A readonly fragment shows the current weight and the goods inside. The receiver line includes its capacity. The sender line is just the current weight. The global-storage fragment stays separate from both workplace panels.

## Workers and haulers

`WorkerRootBehavior` tries workplace behaviors in decorator order and keeps the first one that does not release. Neither building needs a "needs haulers" flag.

- Dispatch Post: the send job, then `FillInputWorkplaceBehavior`. `FillInputHaulBehaviorProvider` lets district haulers fill the same input. The worker carries goods only while the send job is not runnable.
- Parcel Office: the retrieve job, then `RemoveUnwantedStockWorkplaceBehavior`. The worker carries stock out only while the retrieve job is not runnable. Haulers empty the unwanted stock too.

The sender does not remove unwanted stock. The gathering parcel must stay. The receiver does not fill input. Both workplaces stay `Emptiable`.

## Dispatch Post

1. The worker or district haulers fill the active parcel.
2. When that parcel is complete, the office reserves shared weight. While the weight does not fit, the job waits. An incomplete parcel does not hold weight.
3. The job starts. The first work tick takes the goods out of the building and stores the amounts on the component. The timer keeps running.
4. The worker or haulers may fill the next parcel while the timer runs.
5. When the timer ends, the hold is committed into global storage.

`SendHours` stays 3.

Manual parcels can be cancelled. Cancelling the active parcel, or the next one while it is being gathered, drops goods that are still in the building, or goods already held on the component, as a recovered stack. Later queued manuals have no goods yet.

Auto is a reorder point. Tracked amount is the stored global amount plus every parcel on this desk, except the parcel a method is replacing. Outgoing holds are not subtracted. A good reserved for a Parcel Office still counts as stored.

Each pass can keep the active auto and one lookahead auto after the manuals. Extra lookaheads are deleted. The last one stays.

- A new parcel is packed when any rule has tracked amount below `Target - Margin`. The packed amount is the shortage up to `Target`, in rule order, capped at one parcel. The margin decides whether to pack. It does not shrink the packed amount.
- The active auto is packed once. A later stock change does not repack it. `SetAuto` clears that latch when the goods have not been taken, so the next pass repacks it. Confirming rules is that path. A job that has already taken its goods keeps the parcel it took.
- The lookahead is added in the same pass when the tracked amount, including the active parcel, is still below `Target - Margin`. Later passes rebuild it. It is removed when the margin is no longer crossed.
- The active auto is removed when its goods have not been taken, it has no hold, and every good in it is already at `Target` without counting that parcel or the lookahead. That check uses `Target`, not `Target - Margin`. One short good keeps the whole parcel. A job that has taken its goods, or that already holds weight, finishes.

Turning auto off, or confirming rules, deletes every queued auto immediately. Gathered goods for a queued auto that is the gathering parcel are dropped. The active auto is left to the removal rule above. While auto is off, the player can cancel that active parcel.

An in-flight send whose goods are already taken is allowed to finish. The lookahead can be filled while that timer runs.

## Parcel Office

The active parcel reserves those global goods as soon as it is active, and retries when global stock changes. The worker starts only after that hold exists and the building can accept the result. Later parcels are not reserved yet.

- The job waits while stock weight plus parcel weight is over `InventoryCapacity` and the parcel could fit in an empty building. Workers and haulers take goods out first.
- If the parcel is heavier than the building even when empty, including one item heavier than the parcel limit, the job starts anyway. The extra weight sits until it is carried out.
- The timer then runs. At the end, the hold is committed and the goods are put in the building. Nothing is removed from global storage at the start.
- Cancel releases the hold. Goods already delivered stay in the building. The whole stock is unwanted. There is nothing to turn into a recovered stack.

`RetrieveHours` is 3. `InventoryCapacity` is 80 kg.

Receiver auto is a keep list with a margin. Tracked amount is the amount in this building plus every parcel on this desk. Global stock is not added.

- The dialog edits an amount and a margin for each good. It refuses a list whose weight is over `InventoryCapacity`, except one item of amount 1 that is heavier than that cap.
- One auto parcel. Another is not queued while any auto parcel exists. There is no lookahead.
- A new parcel is packed when any keep has tracked amount below `Target - Margin`. Its size is the shortage up to `Target`, in list order, capped at one parcel.
- An existing auto parcel is not resized in place. It is removed when it has no hold and, without that parcel, every good in it is already at `Target - Margin`. One short good keeps it. A hold on the active auto keeps it until the job ends, even when auto is turned off. A parcel that is larger than the new keep stays when the building would still be under the threshold without it.
- Example, with each good weighing 1 kg: keep 10 Logs and 10 Planks, margin 0, empty building. The parcel is as much of that shortage as fits in 60 kg. Those goods count while they are still in the building, so auto does not queue another. When they leave, tracked falls and auto queues again. The keep does not stop haulers from treating them as unwanted.
- Turning auto off, or confirming a new list, deletes a queued auto immediately. The active auto is left to the removal rule above. Goods already in the building stay.

## UI

The sender and the receiver each have their own fragment. There is no shared desk panel.

The manual-order dialog is shared. It edits a list of goods and amounts, shows total weight and parcel count, and confirms. Both fragments open it.

The auto dialogs are separate. Both edit a target and a margin. The receiver dialog also shows that list's weight against `InventoryCapacity` and blocks confirm when the list is too heavy.

Both sidebars use Timberborn controls: Add manual order, Auto order, Configure auto order, the active parcel with a progress bar, then the waiting parcels. A short status line covers the wait reason. Dropdown, integer amount, Add, Confirm, Remove, and Cancel are the controls. While auto is on, the active auto shows the uncheck hint instead of Cancel.

- Sender status: waiting for goods, or waiting for shared weight.
- Receiver status: waiting for global goods, or waiting for room.
