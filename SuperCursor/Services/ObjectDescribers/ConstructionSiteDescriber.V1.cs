namespace SuperCursor.Services.ObjectDescribers;

public class ConstructionSiteDescriber(ILoc t, IGoodService goods) : BaseObjectDescriber<ConstructionSite>
{
    protected override void DescribeComponent(StringBuilder builder, ConstructionSite component)
    {
        if (!component.Enabled) { return; }

        var priority = component.GetComponent<BuilderPrioritizable>();
        var priorityText = priority is null ? "" : (" " + priority.Priority.ToTooltipString());

        builder.AppendLine($"{t.T("ConstructionSites.DisplayName").Bold()}: {component.BuildTimeProgress * 100:F2}%{priorityText}");

        var inventory = component.Inventory;
        if (inventory is null) { return; }

        var materials = inventory.AllowedGoods;
        foreach (var item in materials)
        {
            var id = item.StorableGood.GoodId;

            var name = goods.GetGood(id).PluralDisplayName.Value;
            builder.AppendLine($"  {name}: {inventory.AmountInStock(id)} / {inventory.LimitedAmount(id)}");
        }
    }
}
