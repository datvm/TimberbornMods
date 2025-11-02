namespace SuperCursor.Services.ObjectDescribers;

[ServicePriority(1)]
public class CharacterDescriber(ILoc t, LifeService lifeService) : BaseObjectDescriber<Character>
{

    protected override void DescribeComponent(StringBuilder builder, Character component)
    {
        var age = component.Age;
        builder.AppendLine(t.T("Beaver.Age", age));

        var child = component.GetComponentFast<Child>();
        if (child is not null)
        {
            DescribeChild(builder, child, age);
        }

        var deteriorable = component.GetComponentFast<Deteriorable>();
        if (deteriorable is not null)
        {
            DescribeDeteriorable(builder, deteriorable, age);
        }

        var dweller = component.GetComponentFast<Dweller>();
        if (dweller is not null)
        {
            DescribeDweller(builder, dweller);
        }

        var worker = component.GetComponentFast<Worker>();
        if (worker is not null && child is null)
        {
            DescribeWorker(builder, worker);
        }
    }

    void DescribeDeteriorable(StringBuilder builder, Deteriorable deteriorable, int age)
    {
        var progress = deteriorable.DeteriorationProgress;
        var total = progress == 1 ? 0 : (int)MathF.Ceiling(age / (1 - progress));

        builder.AppendLine($"{t.T("Bot.Durability")}: {age}/{total} ({progress * 100f:F0}%)");
    }

    void DescribeChild(StringBuilder builder, Child child, int age)
    {
        var progress = child.GrowthProgress;
        var total = progress == 0 ? 0 : (int)MathF.Ceiling(age / progress);

        builder.AppendLine(t.T("LV.SC.BeaverChildGrow",
            age,
            lifeService._daysOfChildhood,
            (progress * 100).ToString("F0")));
    }

    void DescribeDweller(StringBuilder builder, Dweller dweller)
    {
        if (dweller.HasHome)
        {
            builder.AppendLine(t.T("Beaver.House", GetDisplayName(dweller.Home)));
        }
        else
        {
            builder.AppendLine(t.T("Beaver.Homeless"));
        }
    }

    void DescribeWorker(StringBuilder builder, Worker worker)
    {
        var workplace = worker.Workplace;
        if (workplace is null)
        {
            builder.AppendLine(t.T("Beaver.Unemployed"));
        }
        else
        {
            builder.AppendLine(t.T("Beaver.Workplace", GetDisplayName(workplace)));
        }
    }

    string GetDisplayName(BaseComponent building)
    {
        return t.T(building.GetComponentFast<LabeledEntitySpec>().DisplayNameLocKey);
    }

}