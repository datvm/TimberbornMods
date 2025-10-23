namespace WeatherScientificProjects.Components;

public class WeatherSPWaterStrengthModifier : BaseComponent, IWaterStrengthModifier, IEntityMultiEffectsDescriber
{
    public bool IsBadwaterSource { get; private set; }
    public float CurrentStrength => waterSource.CurrentStrength;

    ScientificProjectInfo[] activeProjects = [];
    float modifier = 1;
    public float GetStrengthModifier() => modifier;

#nullable disable
    WaterSource waterSource;
    WeatherSPWaterStrengthService service;
#nullable enable

    [Inject]
    public void Inject(WeatherSPWaterStrengthService service)
    {
        this.service = service;
    }

    public void Start()
    {
        var contamination = GetComponentFast<WaterSourceContamination>();
        IsBadwaterSource = contamination.Contamination > 0f;

        waterSource = GetComponentFast<WaterSource>();
        waterSource.AddWaterStrengthModifier(this);

        SetModifier(IsBadwaterSource ? service.CurrentInfo.Bad : service.CurrentInfo.Fresh);
    }

    public void SetModifier(WeatherSPWaterStrengthInfo info)
    {
        activeProjects = info.Projects;

        modifier = Math.Max(0, 1 + info.Modifier);
    }

    public IEnumerable<EntityEffectDescription> DescribeAll(ILoc t, IDayNightCycle dayNightCycle)
    {
        foreach (var p in activeProjects)
        {
            var isIncreasing = WeatherProjectsUtils.WaterStrengthIncreaseIds.Contains(p.Spec.Id);
            var str = p.GetEffect(0);
            if (str == 0) { continue; }

            yield return new(
                t.T(isIncreasing ? "LV.WSP.IncreaseStr" : "LV.WSP.DecreaseStr") + " ×" + p.Levels.Today,
                t.T("LV.WSP.StrBuffEffect", str) + Environment.NewLine
                    + t.T(isIncreasing ? "LV.WSP.IncreaseStrBuffDesc" : "LV.WSP.DecreaseStrBuffDesc")
            );
        }
    }

}
