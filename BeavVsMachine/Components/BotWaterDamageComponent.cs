namespace BeavVsMachine.Components;

public class BotWaterDamageComponent : BaseComponent
{
    public const float WaterDamagePerTick = 1f / 24f; // Multiply with the supplied parameter in hours because deteriorable works in days
    public const float ContaminationMultiplier = 10f; // 10 times more damaging at 100% contamination

#nullable disable
    ModdableSoakEffectComponent soakEffect;
    Deteriorable deteriorable;
#nullable enable

    public void Awake()
    {
        soakEffect = GetComponentFast<ModdableSoakEffectComponent>();
        deteriorable = GetComponentFast<Deteriorable>();

        soakEffect.OnSoakedTick += OnSoakedTick;
    }

    private void OnSoakedTick(object sender, SoakEffectEventArgs e)
    {
        if (e.Resistant) { return; }

        var contamination = e.WaterColumn.Contamination;
        var damage = e.PassedTimeInHours * WaterDamagePerTick * Mathf.Lerp(1f, ContaminationMultiplier, contamination);
        deteriorable._currentDeterioration -= damage;
    }

}
