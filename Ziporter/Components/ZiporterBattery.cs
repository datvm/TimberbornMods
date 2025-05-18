namespace Ziporter.Components;

public class ZiporterBattery : BaseComponent, IBattery
{
    public const int MaxCapacity = 5_000;

    public float Charge { get; private set; }
    public int Capacity { get; } = MaxCapacity;
    public bool IsCharging { get; private set; }

    public void ModifyCharge(float chargeDelta)
    {
        IsCharging = chargeDelta >= 0;
        Charge = Mathf.Clamp(Charge + chargeDelta, 0, Capacity);
    }

    public void Load(float charge)
    {
        Charge = Mathf.Clamp(charge, 0, Capacity);
    }

}
