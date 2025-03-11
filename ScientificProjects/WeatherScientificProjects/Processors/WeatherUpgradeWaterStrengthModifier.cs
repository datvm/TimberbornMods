namespace WeatherScientificProjects.Processors;

public class WeatherUpgradeWaterStrengthModifier : BaseComponent, IWaterStrengthModifier
{

    public bool IsBadwaterSource { get; private set; }

    public float Modifier { get; set; } = 1;
    public float GetStrengthModifier() => Modifier;

    BuffableComponent buffable = null!;
    WaterSource waterSource = null!;
    WaterSourceContamination conta = null!;

    public void Awake()
    {
        conta = GetComponentFast<WaterSourceContamination>() 
            ?? throw new Exception($"{nameof(WeatherUpgradeWaterStrengthModifier)} requires {nameof(WaterSourceContamination)} component");
        waterSource = GetComponentFast<WaterSource>()
            ?? throw new Exception($"{nameof(WeatherUpgradeWaterStrengthModifier)} requires {nameof(WaterSource)} component");
        buffable = this.GetBuffable();
    }

    public void Start()
    {
        waterSource.AddWaterStrengthModifier(this);
        IsBadwaterSource = conta.Contamination > 0f;

        buffable.OnBuffAdded += Buffable_OnBuffAdded;
        buffable.OnBuffRemoved += Buffable_OnBuffRemoved;
    }

    private void Buffable_OnBuffRemoved(object sender, BuffInstance e)
    {
        if (e is not WeatherWaterSourceBuffInstance ws) { return; }

        Modifier -= ws.Effect.Value;
    }

    private void Buffable_OnBuffAdded(object sender, BuffInstance e)
    {
        if (!e.Active || e is not WeatherWaterSourceBuffInstance ws) { return; }

        Modifier += ws.Effect.Value;
    }

}
