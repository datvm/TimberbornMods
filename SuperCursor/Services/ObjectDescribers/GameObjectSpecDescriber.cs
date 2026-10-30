namespace SuperCursor.Services.ObjectDescribers;

[CursorService(int.MaxValue, true)]
public class GameObjectSpecDescriber(
    ISpecService specs, 
    ILoc t,
    EffectProbabilityService effectProbabilityService
) : BaseObjectDescriber<BaseComponent>, ILoadableSingleton
{
    FrozenDictionary<string, NeedSpec> needs = null!;

    public void Load()
    {
        needs = specs.GetSpecs<NeedSpec>().ToFrozenDictionary(n => n.Id);
    }

    protected override void DescribeComponent(StringBuilder builder, BaseComponent component)
    {
        // Current 5 types of INeedEffectsSpec
        if (component.GetComponent<AreaNeedApplierSpec>() is { } ans)
        {
            AddNeedDescription(ans.Effects, builder, "AreaNeedApplier");
        }

        if (component.GetComponent<DemolishableEffectsSpec>() is { } des)
        {
            AddNeedDescription(des.Effects, builder, "DemolisherNeedApplier");
        }

        if (component.GetComponent<WorkshopRandomNeedApplierSpec>() is { } wrnas)
        {
            AddNeedDescription(wrnas.Effects, builder, "WorkshopRandomNeedApplier");
        }

        if (component.GetComponent<YieldRemoverWorkplaceEffectsSpec>() is { } yrwes)
        {
            AddNeedDescription(yrwes.Effects, builder, "YieldRemoverNeedApplier");
        }

        if (component.GetComponent<YielderNeedApplierSpec>() is { } yrns)
        {
            AddNeedDescription(yrns.Effects, builder, "YieldRemoverNeedApplier");
        }
    }

    void AddNeedDescription(IReadOnlyList<NeedApplierEffectSpec> efs, StringBuilder builder, string id)
    {
        foreach (var ef in efs)
        {
            var name = needs[ef.NeedId].DisplayName.Value;
            var propType = ef.Probability;
            var chance = effectProbabilityService.GetEffectProbability(ef, id);

            builder.AppendLine(t.T("LV.SC.NeedEffect", chance, name, propType.ToString()));
        }
    }
}
