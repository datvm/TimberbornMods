using ModdableTimberborn.ModdableWaterSource;

namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class BuffWaterSourceHandler(
    WaterSourceEntityBuffService waterSourceBuffService
) : NodeHandlerBase<BuffWaterSourceData>
{
    public override string ForType => "BuffWaterSource";

    protected override string? InternalHandleNode(BuffWaterSourceData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var info = data.BuffInfo;
        var days = controller.FormatTextFloat(info.Days);

        if (info.Category != EntityBuffCategory.Permanent && days <= 0)
        {
            waterSourceBuffService.RemoveWaterSourceBuff(info.BuffId);
        }
        else
        {
            waterSourceBuffService.AddOrUpdateWaterSourceBuff(new()
            {
                Id = info.BuffId,
                EntityIds = GetEntityIds(data, controller),
                Effects = GetEffects(data.Effects, controller),
                Title = controller.FormatTextLoc(info.TitleLoc),
                Description = controller.FormatTextLoc(info.DescLoc),
                Category = info.Category,
            }, days);
        }

        return node.NextNodeId;
    }

    static Guid[]? GetEntityIds(BuffWaterSourceData data, SpecChronicleEventController controller)
    {
        if (data.EntityIds is null) { return null; }

        var entityIds = data.EntityIds.Value.Select(id => controller.FormatText(id)).OfType<string>();
        return [.. controller.GetEntities<ModdableWaterSourceComponent>(entityIds).Select(e => e.GetEntityId()).Distinct()];
    }

    static WaterSourceBuffEffects GetEffects(BuffWaterSourceEffects effects, SpecChronicleEventController controller)
        => new(
            effects.ImmuneToDrought is null ? null : controller.FormatTextBool(effects.ImmuneToDrought),
            effects.ImmuneToBadtide is null ? null : controller.FormatTextBool(effects.ImmuneToBadtide),
            effects.StrengthMultiplier is null ? null : controller.FormatTextFloat(effects.StrengthMultiplier),
            effects.ContaminationDelta is null ? null : controller.FormatTextFloat(effects.ContaminationDelta)
        );
}
