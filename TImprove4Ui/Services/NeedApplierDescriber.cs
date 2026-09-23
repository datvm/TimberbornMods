namespace TImprove4Ui.Services;

[AddTemplateModule2(typeof(AreaNeedApplier))]
[AddTemplateModule2(typeof(WorkshopRandomNeedApplier), AlsoBindTransient = false)]
public class NeedApplierDescriber : BaseComponent, IEntityDescriber, IAwakableComponent
{

#nullable disable
    FactionNeedSpecService needs;

    AreaNeedApplierSpec areaNeedSpec;
    WorkshopRandomNeedApplierSpec workshopRandomNeedApplierSpec;
    EffectProbabilityService effectProbabilityService;
    MSettings settings;
#nullable enable

    [Inject]
    public void Inject(FactionNeedSpecService needs, MSettings settings, EffectProbabilityService effectProbabilityService)
    {
        this.needs = needs;
        this.settings = settings;
        this.effectProbabilityService = effectProbabilityService;
    }

    public void Awake()
    {
        areaNeedSpec = GetComponent<AreaNeedApplierSpec>();
        workshopRandomNeedApplierSpec = GetComponent<WorkshopRandomNeedApplierSpec>();
    }

    public IEnumerable<EntityDescription> DescribeEntity()
    {
        if (!settings.AddNegativeNeeds.Value) { yield break; }

        if (areaNeedSpec?.Effects.Length > 0)
        {
            foreach (var eff in areaNeedSpec.Effects)
            {
                yield return DescribeEffect(eff, nameof(AreaNeedApplier));
            }

        }

        if (workshopRandomNeedApplierSpec?.Effects.Length > 0)
        {
            foreach (var eff in workshopRandomNeedApplierSpec.Effects)
            {
                yield return DescribeEffect(eff, nameof(WorkshopRandomNeedApplier));
            }
        }
    }

    const string ArrowDown = "▼";
    EntityDescription DescribeEffect(NeedApplierEffectSpec spec, string groupId)
    {
        var need = needs.NeedsByIds[spec.NeedId];
        var needGrp = needs.NeedGroupsByIds[need.NeedGroupId];

        var favorable = spec.Points > 0;
        var text = string.Format("{0} {1}: {2} {3} ({4:0.00%}/h)",
            SpecialStrings.RowStarter,
            needGrp.DisplayName.Value,
            need.DisplayName.Value,
            favorable ? SpecialStrings.ArrowUp : ArrowDown,
            effectProbabilityService.GetEffectProbability(spec, groupId)
        );

        return EntityDescription.CreateTextSection(
            text.Color(favorable ? TimberbornTextColor.Green : TimberbornTextColor.Red),
            1011);
    }

}