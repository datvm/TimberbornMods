namespace TImprove4Ui.Services;

public class NeedApplierDescriber : BaseComponent, IEntityDescriber
{

#nullable disable
    FactionNeedSpecService needs;

    AreaNeedApplierSpec areaNeedSpec;
    WorkshopRandomNeedApplierSpec workshopRandomNeedApplierSpec;
    MSettings settings;
#nullable enable

    [Inject]
    public void Inject(FactionNeedSpecService needs, MSettings settings)
    {
        this.needs = needs;
        this.settings = settings;
    }

    public void Awake()
    {
        areaNeedSpec = GetComponentFast<AreaNeedApplierSpec>();
        workshopRandomNeedApplierSpec = GetComponentFast<WorkshopRandomNeedApplierSpec>();
    }

    public IEnumerable<EntityDescription> DescribeEntity()
    {
        if (!settings.AddNegativeNeeds.Value) { yield break; }

        if (areaNeedSpec && areaNeedSpec.EffectPerHour is not null)
        {
            yield return DescribeEffect(areaNeedSpec.EffectPerHour);
        }

        if (workshopRandomNeedApplierSpec && workshopRandomNeedApplierSpec._effects is not null)
        {
            foreach (var eff in workshopRandomNeedApplierSpec._effects)
            {
                yield return DescribeEffect(eff);
            }
        }
    }

    const string ArrowDown = "▼";
    EntityDescription DescribeEffect(NeedApplierEffectSpecPerHour spec)
    {
        var need = needs.NeedsByIds[spec.NeedId];
        var needGrp = needs.NeedGroupsByIds[need.NeedGroupId];

        var favorable = spec._points > 0;
        var text = string.Format("{0} {1}: {2} {3} ({4:0.00%}/h)",
            SpecialStrings.RowStarter,
            needGrp.DisplayName.Value,
            need.DisplayName.Value,
            favorable ? SpecialStrings.ArrowUp : ArrowDown,            
            spec.Probability
        );

        return EntityDescription.CreateTextSection(
            text.Color(favorable ? TimberbornTextColor.Green : TimberbornTextColor.Red),
            1011);
    }

}