namespace SuperCursor.Services.ObjectDescribers;

public class InventoryDescriber(ILoc t, IGoodService goods) : BaseObjectDescriber<Inventories>
{

    protected override void DescribeComponent(StringBuilder builder, Inventories component)
    {
        var invs = component.EnabledInventories;
        if (invs.Count == 0) { return; }

        var single = invs.Count == 1;

        foreach (var inv in invs)
        {
            var name = t.T("LV.SC.Inventory");
            if (!single)
            {
                name += $" ({inv.ComponentName})";
            }
            builder.AppendLine(name.Bold());

            foreach (var good in inv.Stock)
            {
                if (good.Amount == 0) { continue; }

                builder.AppendLine($"- {good.Amount}× {goods.GetGood(good.GoodId).DisplayName.Value}");
            }
        }
    }

}
