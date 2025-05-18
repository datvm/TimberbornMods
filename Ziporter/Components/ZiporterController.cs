namespace Ziporter.Components;

public class ZiporterController : BaseComponent, IPersistentEntity
{
    static readonly ComponentKey ZiporterComponentSaveKey = new("Ziporter");

    static readonly PropertyKey<float> ChargeKey = new("Charge");
    static readonly PropertyKey<float> StabilizerKey = new("Stabilizer");
    static readonly PropertyKey<bool> FinishedStateKey = new("FinishedState");
    static readonly PropertyKey<Guid> ConnectionIdKey = new("ConnectionId");

    public float Charge => battery.Charge;
    public bool IsCharging => battery.IsCharging;

    public float Stabilizer => stablizer.Stabilizer;
    public float StabilizerPercent => stablizer.StabilizerPercent;
    public bool IsStabilizerCharging => stablizer.IsStabilizerCharging;

    public ZiporterConnection Connection => connection;

    #region References

#nullable disable
    ZiporterStabilizer stablizer;
    ZiporterBattery battery;
    ZiporterConnection connection;

    public void Awake()
    {
        stablizer = GetComponentFast<ZiporterStabilizer>();
        battery = GetComponentFast<ZiporterBattery>();
        connection = GetComponentFast<ZiporterConnection>();
    }

#nullable enable

    #endregion

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(ZiporterComponentSaveKey, out var s)) { return; }

        if (s.Has(ChargeKey))
        {
            battery.Load(s.Get(ChargeKey));
        }

        if (s.Has(StabilizerKey))
        {
            stablizer.Load(
                s.Get(StabilizerKey),
                s.Has(FinishedStateKey) && s.Get(FinishedStateKey));
        }

        if (s.Has(ConnectionIdKey))
        {
            var connId = s.Get(ConnectionIdKey);
            connection.Load(connId);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(ZiporterComponentSaveKey);
        s.Set(ChargeKey, Charge);
        s.Set(StabilizerKey, Stabilizer);

        if (stablizer.IsEverFinished)
        {
            s.Set(FinishedStateKey, true);
        }

        var connId = Connection?.ConnectedZiporter?.GetComponentFast<EntityComponent>()?.EntityId;
        if (connId is not null)
        {
            s.Set(ConnectionIdKey, connId.Value);
        }
    }

    public void SetStabilizerPerc(int perc) => stablizer.SetPercent(perc);

}
