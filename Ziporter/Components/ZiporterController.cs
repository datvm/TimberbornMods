namespace Ziporter.Components;

public class ZiporterController : BaseComponent, IPersistentEntity
{
    static readonly ComponentKey ZiporterComponentSaveKey = new("Ziporter");

    static readonly PropertyKey<float> ChargeKey = new("Charge");
    static readonly PropertyKey<float> StabilizerKey = new("Stabilizer");
    static readonly PropertyKey<bool> FinishedStateKey = new("FinishedState");

    public float Charge => battery.Charge;
    public float Stabilizer => stablizer.Stabilizer;
    public float StabilizerPercent => stablizer.StabilizerPercent;
    public bool IsStabilizerCharging => stablizer.IsStabilizerCharging;

    #region References

#nullable disable
    ZiporterStabilizer stablizer;
    ZiporterBattery battery;

    public void Awake()
    {
        stablizer = GetComponentFast<ZiporterStabilizer>();
        battery = GetComponentFast<ZiporterBattery>();
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
    }
}
