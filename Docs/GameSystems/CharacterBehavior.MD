# Character Behavior

Timberborn character behavior is a per-character priority scheduler.

Each character has a `BehaviorManager`, a `BehaviorAgent`, and an ordered list of `RootBehavior` components. On each tick, the manager either continues the currently running `IExecutor` or asks the root behaviors, in order, whether they can act.

Behaviors do not perform long-running work directly. A `Behavior` returns a `Decision`, and that decision may start an `IExecutor` such as walking, waiting, entering a building, or delivering goods. The executor is then ticked until it succeeds, fails, or keeps running.

## Core Types

- `BehaviorManager`: ticks the current executor, scans root behaviors, tracks the running behavior, and persists behavior state.
- `BehaviorAgent`: a lightweight component passed into behavior decisions.
- `Behavior`: base class with `Decide(BehaviorAgent agent)`.
- `RootBehavior`: a high-level behavior registered directly with `BehaviorManager`.
- `Decision`: tells the manager whether to run an executor, return to the same behavior, release to lower-priority behaviors, or try again next tick.
- `IExecutor`: performs ongoing work over multiple ticks and reports `Running`, `Success`, or `Failure`.

## Beaver Priority

Beavers register root behaviors in this order:

1. `CharacterControlRootBehavior`
2. `DeadRootBehavior`
3. `CarryRootBehavior` for adults, or `ChildRootBehavior` for children
4. `DieRootBehavior`
5. `ContaminateRootBehavior`
6. `CriticalNeederRootBehavior`
7. `StrandedRootBehavior`
8. `WorkerRootBehavior` for adults
9. `NeederRootBehavior`
10. `WanderRootBehavior`

This means manual control, death, carrying, critical needs, and stranded handling can preempt normal work or idle behavior. Wandering is the fallback when nothing more important wants to act.

## Bot Priority

Bots use a similar stack:

1. `CharacterControlRootBehavior`
2. `DeadRootBehavior`
3. `CarryRootBehavior`
4. `DieRootBehavior`
5. `CriticalNeederRootBehavior`
6. `StrandedRootBehavior`
7. `NeederRootBehavior`
8. `WorkerRootBehavior`
9. `WanderRootBehavior`

The notable difference is that normal needs are checked before work.

## Needs

Need handling is routed through `INeedBehaviorPicker`.

For beavers, `SleepNeedBehavior` is registered as the essential behavior. The picker reserves time for that essential action, then asks the district's `DistrictNeedBehaviorService` for the best non-essential need action based on appraised need value, position, and available time.

Critical needs are handled by `CriticalNeederRootBehavior`, which runs before normal work. Ordinary needs are handled later by `NeederRootBehavior`.

## Work

`WorkerRootBehavior` only acts when the character is willing to work, employed, and inside working hours. It delegates to the assigned workplace's `WorkplaceBehavior` list and returns the first workplace behavior that can produce a non-release decision.

If the worker is not employed or not in working hours, it can fall back to community service behavior.

## Mental Model

The system is not one large AI controller. It is a layered priority list:

1. High-level root behaviors decide whether they currently apply.
2. Some roots delegate to lower-level behaviors, such as need behaviors or workplace behaviors.
3. Decisions launch executors for long-running actions.
4. When an executor finishes, the manager reconsiders the behavior stack.

For modding, this means behavior changes usually involve either adding/reordering a root behavior, changing a lower-level behavior's `Decision`, or altering the services that pick need or workplace actions.
