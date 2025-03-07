namespace ScientificProjects.Buffs;

public class BeaverBuilderBuffTarget(IBuffableService buffables, EventBus eventBus) : GlobalAdultBeaverBuffTarget(buffables, eventBus)
{

    protected override bool Filter(BuffableComponent buffable)
    {
        if (!base.Filter(buffable)) { return false; }

        var worker = buffable.GetComponentFast<Worker>();
        var workplace = worker?.Workplace;
        if (workplace is null) { return false; }

        return workplace.GetComponentFast<DistrictCenter>() is not null ||
            workplace.GetComponentFast<BuilderHubSpec>() is not null;
    }

}
