namespace BrainPowerSPs.Buffs;

public interface IPowerFlatEffect : IBuffEffect
{
    bool Enabled { get; }
    float Value { get; }
}

public interface IPowerMultiplierEffect : IBuffEffect
{
    bool Enabled { get; }
    float Value { get; }
}

public interface IPowerCustomMultiplierEffect: IBuffEffect
{
    bool Enabled { get; }
    float GetValue(BuffableComponent buffable);
}

public class PowerOutputFlatBuffEff(float value, string name, ILoc t) : SimpleValueBuffEffect<float>(value), IPowerFlatEffect
{
    public bool Enabled { get; } = true;

    protected override string? GetDescription(float value) => string.Format(t.T("LV.BPSP.PowerOutputFlatEff"), value, name);
}

public class PowerOutputMultiplierBuffEff(float value, ILoc t) : SimpleValueBuffEffect<float>(value), IPowerMultiplierEffect
{
    public bool Enabled { get; } = true;

    protected override string? GetDescription(float value) => string.Format(t.T("LV.BPSP.PowerOutputMulEff"), value);
}

public class PowerOutputDayMultiplierBuffEff(Vector2 values, IDayNightCycle dayNight, ILoc t) : IBuffEffect, IPowerMultiplierEffect
{

    public bool Enabled { get; } = true;
    public float Value => values[0] + (values[1] - values[0]) * dayNight.DayProgress;
    public string? Description => t.T("LV.BPSP.PowerOutputMulDayEff", Value, values[0], values[1]);

    public long Id { get; set; }
    

    public void CleanUp() { }
    public void Init() { }
    public void UpdateEffect() { }
}

public class GeneratorMinStrengthBuffEff(float value, string name, ILoc t) : SimpleValueBuffEffect<float>(value)
{

    protected override string? GetDescription(float value) => string.Format(t.T("LV.BPSP.GeneratorStrMinEff"), value, name);
}

public class WindmillHeightBuffEff(float value, ILoc t) : SimpleValueBuffEffect<float>(value), IPowerCustomMultiplierEffect
{
    public bool Enabled { get; } = true;

    public float GetValue(BuffableComponent buffable)
    {
        var windmill = buffable.GetComponentFast<WindmillBuffComponent>();
        if (!windmill) { return 0; }

        return Value * windmill.PeakHeight;
    }

    protected override string? GetDescription(float value) => string.Format(t.T("LV.BPSP.WindmillHeightBuffEff"), value);
}