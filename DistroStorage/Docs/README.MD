# DistroStorage extension guide

Other mods can integrate with DistroStorage by adding a component that implements `IDistroSender` or `IDistroReceiver`. The easiest path is usually to inherit from `DistroComponentBase`, then implement the sender- or receiver-specific members.

Inventory-backed buildings can use the more specific inventory base classes:

- `InventoryDistroSenderBase`
- `InventoryDistroReceiverBase`

Non-inventory integrations should use `DistroComponentBase` directly.

## How transfers work

DistroStorage periodically ticks every registered sender. For each active sender, it:

1. Reads the sender's available `Goods`.
2. Checks the sender's connected receivers in `GetPrioritizedReceivers()` order.
3. Calls `receiver.CanReceiveGood(senderGoodIds)`.
4. If a receiver returns a good ID, transfers one item:

```cs
sender.TransferOut(new(goodId, 1));
receiver.TransferIn(new(goodId, 1));
```

This means a receiver does not need an `Inventory`. It only needs to answer whether it can accept a good, and then handle that good in `TransferIn`.

## Shared component contract

All senders and receivers implement `IDistroComponent`.

### State

- `IsSender` and `IsReceiver`: identify which side of the transfer this component represents.
- `SystemDisabled`: `true` when the integration should be unavailable regardless of user settings.
- `Enabled`: user-controlled enabled state.
- `Active`: calculated runtime state. In `DistroComponentBase`, this is based on `SystemDisabled`, preview state, and finished-building state.
- `ActiveAndEnabled`: convenience check used by transfer logic.
- `Bounds`: block bounds used to find nearby senders or receivers.

### Goods

- `Goods`: goods this component can send or receive.
- `GoodIds`: IDs from `Goods`, usually implemented as `Goods.Select(g => g.GoodId)`.

For senders, `Goods` means available unreserved stock.

For receivers, `Goods` means wanted capacity or remaining demand.

### Connections

- `Connections`: connected components in distro range.
- `ConnectWith(IDistroComponent component)`: called by the registry.
- `DisconnectFrom(IDistroComponent component)`: called when components unregister.
- `ClearConnections()`: clears all current links.

Mods generally should not call these methods directly.

### Enabled state

- `SetEnabled(bool enabled)`: changes the component enabled state and invalidates activity.

## `DistroComponentBase`

`DistroComponentBase` provides registration, persistence for `Enabled`, preview disabling, finished-building activity checks, and range connection storage.

Use it like this:

```cs
public class MyReceiver(DistroService service) : DistroComponentBase(service), IDistroReceiver
{
}
```

Important overridable members:

- `SystemDisabled`: return `true` when a setting, missing dependency, or invalid building state should disable the component.
- `EnabledByDefault`: default user-enabled state for new entities.
- `RequireFinishedBuilding`: defaults to `true`. Set to `false` for construction-site style receivers.
- `CalculateActive()`: add runtime checks such as paused state, input availability, or remaining demand.
- `Awake()`: get required components, then call `base.Awake()` unless your component must be fetched before base logic.
- `InitializeEntity()`: call `base.InitializeEntity()` if overridden, because it sets `Bounds` and registers the component.
- `DeleteEntity()`: call `base.DeleteEntity()` if overridden, because it unregisters the component.
- `Save(IEntitySaver entitySaver)` and `Load(IEntityLoader entityLoader)`: call base methods to preserve `Enabled`.

Call `MarkActiveDirty()` when something changes that affects `Active`, `Goods`, or availability.

## Receivers

Receivers implement `IDistroReceiver`, which extends `IDistroComponent` and Timberborn's `IPrioritizable`.

Required members:

- `Priority Priority`: used by senders when ordering receivers.
- `SetPriority(Priority priority)`: updates the receiver priority.
- `CanReceiveGood(HashSet<string> goodIds)`: returns a good ID this receiver wants, or `null`.
- `TransferIn(GoodAmount good)`: accepts the transferred item.
- `Serialize()` and `Deserialize(DistroReceiverSerializableModel model)`: used by building settings duplication.

Minimal non-inventory receiver:

```cs
public class MyDemandReceiver(DistroService service) : DistroComponentBase(service), IDistroReceiver
{
    static readonly ComponentKey ReceiverSaveKey = new(nameof(MyDemandReceiver));
    static readonly PropertyKey<int> PriorityKey = new(nameof(Priority));

    MyDemandComponent demand = null!;

    public override IEnumerable<GoodAmount> Goods => demand.RemainingGoods;
    public override IEnumerable<string> GoodIds => Goods.Select(g => g.GoodId);

    public Priority Priority { get; private set; } = Priority.Normal;

    public override void Awake()
    {
        base.Awake();
        demand = GetComponent<MyDemandComponent>();
    }

    protected override bool CalculateActive()
        => base.CalculateActive()
        && Goods.Any(g => g.Amount > 0);

    public string? CanReceiveGood(HashSet<string> goodIds)
        => Goods.FirstOrDefault(g => g.Amount > 0 && goodIds.Contains(g.GoodId)).GoodId;

    public void TransferIn(GoodAmount good)
    {
        demand.Pay(good.GoodId, good.Amount);
        MarkActiveDirty();
    }

    public void SetPriority(Priority priority) => Priority = priority;

    public override void Save(IEntitySaver entitySaver)
    {
        base.Save(entitySaver);

        var s = entitySaver.GetComponent(ReceiverSaveKey);
        s.Set(PriorityKey, (int)Priority);
    }

    public override void Load(IEntityLoader entityLoader)
    {
        base.Load(entityLoader);

        if (!entityLoader.TryGetComponent(ReceiverSaveKey, out var s))
        {
            return;
        }

        if (!s.Has(PriorityKey))
        {
            return;
        }

        Priority = (Priority)s.Get(PriorityKey);
    }

    public DistroReceiverSerializableModel Serialize() => new(Enabled, Priority);

    public void Deserialize(DistroReceiverSerializableModel model)
    {
        if (model.Enabled != Enabled)
        {
            SetEnabled(model.Enabled);
        }

        if (model.Priority != Priority)
        {
            SetPriority(model.Priority);
        }
    }
}
```

For inventory-backed receivers, inherit from `InventoryDistroReceiverBase` and implement `Inventory`. It already handles priority, serialization, capacity checks, and `Inventory.GiveExisting`.

## Senders

Senders implement `IDistroSender`.

Required members:

- `Timer`: controls transfer timing.
- `TransferOut(GoodAmount good)`: removes the transferred item from the sender.
- `GetPrioritizedReceivers()`: returns connected receivers in preferred order.
- `Serialize()` and `Deserialize(DistroSenderSerializableModel model)`: used by building settings duplication.

Convenience members from `IDistroSender`:

- `NextTransferTime`: current timer value.
- `TransferProgress`: progress from `0` to `1` until the next transfer.

Minimal non-inventory sender:

```cs
public class MySupplySender(DistroService service) : DistroComponentBase(service), IDistroSender
{
    static readonly ComponentKey SenderSaveKey = new(nameof(MySupplySender));

    MySupplyComponent supply = null!;

    public override IEnumerable<GoodAmount> Goods => supply.AvailableGoods;
    public override IEnumerable<string> GoodIds => Goods.Select(g => g.GoodId);

    public IDistroSenderTimer Timer { get; } = new DefaultDistroSenderTimer(service.TransferTime);

    public override void Awake()
    {
        base.Awake();
        supply = GetComponent<MySupplyComponent>();
    }

    protected override bool CalculateActive()
        => base.CalculateActive()
        && Goods.Any(g => g.Amount > 0);

    public IEnumerable<IDistroReceiver> GetPrioritizedReceivers() => Connections
        .Where(c => c.ActiveAndEnabled)
        .Cast<IDistroReceiver>()
        .OrderByDescending(r => r.Priority);

    public void TransferOut(GoodAmount good)
    {
        supply.Take(good.GoodId, good.Amount);
        MarkActiveDirty();
    }

    public override void Save(IEntitySaver entitySaver)
    {
        base.Save(entitySaver);

        var s = entitySaver.GetComponent(SenderSaveKey);
        Timer.Save(s);
    }

    public override void Load(IEntityLoader entityLoader)
    {
        base.Load(entityLoader);

        if (entityLoader.TryGetComponent(SenderSaveKey, out var s))
        {
            Timer.Load(s);
        }
    }

    public DistroSenderSerializableModel Serialize() => new(Enabled);

    public void Deserialize(DistroSenderSerializableModel model)
    {
        if (model.Enabled != Enabled)
        {
            SetEnabled(model.Enabled);
        }
    }
}
```

For inventory-backed senders, inherit from `InventoryDistroSenderBase` and implement `Inventory` plus the sender save key. It already handles available stock, timer persistence, priority ordering, and `Inventory.TakeExisting`.

## Timers

Use `DefaultDistroSenderTimer` unless you need custom timing behavior.

```cs
public IDistroSenderTimer Timer { get; } = new DefaultDistroSenderTimer(service.TransferTime);
```

`IDistroSenderTimer` members:

- `NextTransfer`: hours remaining before the next transfer.
- `BaseHoursPerItem`: base transfer time.
- `EffectiveHoursPerItem`: transfer time after modifiers.
- `Reset()`: resets `NextTransfer` to `EffectiveHoursPerItem`.
- `SetModifier(string id, float multiplier)`: adds or replaces a timing modifier.
- `RemoveModifier(string id)`: removes a timing modifier.
- `Load(IObjectLoader s)` and `Save(IObjectSaver s)`: persists timer state.

Modifiers multiply together. For example, `0.5f` makes transfers twice as fast, while `2f` makes transfers twice as slow.

## Building settings and duplication

`Serialize()` and `Deserialize(...)` are for copying settings between duplicated buildings and building settings tools.

The default models are:

```cs
public record DistroSenderSerializableModel(bool Enabled);
public record DistroReceiverSerializableModel(bool Enabled, Priority Priority);
```

If your component has additional settings, store them in your own persistent component state. The public distro settings models only carry the common DistroStorage settings.

## Common patterns

Use `DistroComponentBase` directly when:

- The sender or receiver is not backed by `Inventory`.
- Receiving a good means paying, consuming, completing progress, or triggering custom state.
- Sending a good means draining a custom resource.

Use `InventoryDistroReceiverBase` when:

- The building has an input `Inventory`.
- Receiving should call `Inventory.GiveExisting`.
- Capacity should be based on unreserved inventory space.

Use `InventoryDistroSenderBase` when:

- The building has an `Inventory`.
- Sending should call `Inventory.TakeExisting`.
- Available goods should be based on unreserved takeable stock.

Avoid implementing `IDistroComponent` from scratch unless you need to replace registration, range connection, preview, persistence, and finished-building behavior.
