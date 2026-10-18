# The Post Office

## Building

The Post Office is a workplace. The building plan is the Folktails Hauling Post, `Buildings/DistrictManagement/HaulingPost/HaulingPost.Folktails.blueprint.json`.

The mod uses Global Inventory through `Targets/CommonGlobalInventory.targets`.

Haulers bring goods to the Post Office. Haulers remove goods from the Post Office. A Stockpile uses this same flow.

The storage is one dynamic inventory. It follows the Crane `CraneInventory` pattern. The vanilla `Inventory` changes its allowed goods and amounts when the queue changes.

## Terms

- **Send.** A worker moves goods from the Post Office into the global inventory.
- **Retrieve.** A worker moves goods from the global inventory into the Post Office inventory.
- **Parcel.** One pack of goods that one worker sends or retrieves in one action. The goods in a parcel can differ.
- **Queue.** The ordered list of parcels on one Post Office. Every parcel in the queue is a send, or every parcel is a retrieve.
- **Keep amount.** The target amount for an automatic parcel.
- **Blocked.** The front parcel keeps its progress. The workplace offers no work, so the worker stays idle.
- **Shared weight limit.** The maximum stored weight for all goods in the global inventory. Each finished Post Office adds `PostOfficeSpec.GlobalCapacityContribution`.

## Parcel

`PostOfficeConfigSpec.ParcelWeight` is the maximum weight of one parcel.

The player names goods and amounts. Those amounts are split, in that order, into parcels. Two Logs and two Planks share one parcel when their combined weight fits. A parcel of weight 1 takes the same time as a full parcel.

`PostOfficeSpec.SendHours` is the send time. `PostOfficeSpec.RetrieveHours` is the retrieve time. Worker speed and the work bonus decrease that time.

## Queue

The player adds a send or a retrieve. The player cancels a queued parcel, or the parcel in progress.

The queue has one direction. A send can be added while the queue is sending or empty. A retrieve can be added while the queue is retrieving or empty. The other direction waits until the queue is empty.

Switching direction leaves the goods in the inventory. Goods that belong to an open parcel stay. Other goods are unwanted, and haulers take them away.

The front parcel is the only one a worker can progress.

## Inventory

`PostOfficeSpec.InventoryCapacity` is the inventory weight limit, in kilograms.

On a send, the inventory accepts goods for every queued parcel, front first, until the next good would pass the weight limit. The last accepted parcel can be partial. The worker starts the front parcel only when every good of that parcel is in the inventory.

On a retrieve, the front parcel starts only when the inventory can hold that whole parcel.

## Send

1. Haulers fill the open parcels up to the inventory weight limit.
2. When the front parcel is complete in the inventory, the Post Office reserves shared weight for it. While the weight does not fit, the parcel is blocked.
3. The worker then runs the send time. At the end, the goods move into the global inventory and the reserve is consumed.
4. Cancel during the send, including while blocked, releases the reserve. Global amounts stay as they are. The goods in the building become unwanted.

## Retrieve

Retrieve does not reserve global goods.

1. The front parcel starts when the inventory can hold it.
2. While the global inventory lacks those goods, the parcel is blocked at its current progress, including 100%.
3. When the goods are there again, the worker continues from that progress. At 100%, the goods move into the Post Office inventory on that event.

Each Post Office progresses its own front parcel. Two offices can retrieve the same good at the same time. The office that finishes while the goods are there takes them. The other stays blocked at its saved progress.

## Auto

There is one threshold. An auto parcel is added when the tracked amount is under the keep amount. The parcel size is the shortage, capped at one parcel. It stops being added once the tracked amount reaches the keep amount.

Auto keeps at most one of its parcels in the queue. When that parcel leaves the queue, the same check can add the next one.

Auto send tracks the global amount. Auto retrieve tracks the amount in this Post Office inventory.

Auto uses the queue direction. While the queue has parcels of the other direction, auto waits.

## Events

The board does not tick. It reacts when:

- the inventory stock changes
- a global amount changes
- the shared weight limit changes
- the player edits the queue
- the worker reports progress while the front parcel is runnable

A runnable send has its goods in the inventory and its shared weight reserved. A runnable retrieve fits in the inventory, the global goods are there, and its progress is under 100%.
