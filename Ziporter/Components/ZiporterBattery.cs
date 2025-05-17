namespace Ziporter.Components;

public class ZiporterBattery : BaseComponent, IBattery
{
    public const int MaxCapacity = 10_000;

    public float Charge { get; private set; }
    public int Capacity { get; } = MaxCapacity;

    public void ModifyCharge(float chargeDelta)
    {
        Charge = Mathf.Clamp(Charge + chargeDelta, 0, Capacity);
    }

    public void Load(float charge)
    {
        Charge = Mathf.Clamp(charge, 0, Capacity);
    }

}
