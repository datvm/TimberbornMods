namespace BrainPowerSPs.Buffs;

public class PowerOutputFlatBuffEff(float value, string name, ILoc t) : SimpleValueBuffEffect<float>(value)
{
    protected override string? GetDescription(float value) => string.Format(t.T("LV.BPSP.PowerOutputFlatEff"), value, name);
}

public class PowerOutputMultiplierBuffEff(float value, ILoc t) : SimpleValueBuffEffect<float>(value)
{
    protected override string? GetDescription(float value) => string.Format(t.T("LV.BPSP.PowerOutputMulEff"), value);
}

public class PowerOutputDayMultiplierBuffEff(Vector2 values, IDayNightCycle dayNight, ILoc t) : IBuffEffect
{

    public float CurrentValue => values[0] + (values[1] - values[0]) * dayNight.DayProgress;
    public string? Description => t.T("LV.BPSP.PowerOutputMulDayEff", CurrentValue, values[0], values[1]);

    public long Id { get; set; }

    public void CleanUp() { }
    public void Init() { }
    public void UpdateEffect() { }
}

public class GeneratorMinStrengthBuffEff(float value, string name, ILoc t) : IBuffEffect
{
    readonly float value = value;
    public float Value { get; } = value;

    public bool Enabled { get; set; }
    public string? Description => Enabled ? 
        string.Format(t.T("LV.BPSP.GeneratorStrMinEff"), value, name) : 
        string.Format(t.T("LV.BPSP.GeneratorStrMinEffNoWater"), name);

    public long Id { get; set; }
    public void CleanUp() { }
    public void Init() { }
    public void UpdateEffect() { }

}