namespace SuperCursor.Services.ObjectDescribers;

public class StockpileDescriber(ILoc loc, IGoodService goods) : BaseObjectDescriber<Stockpile>
{
    public const string NothingSelectedLocKey = "Inventory.NothingSelected";

    protected override void DescribeComponent(StringBuilder builder, Stockpile component)
    {
        var goodAllower = component.GetComponentFast<SingleGoodAllower>();
        var id = goodAllower.AllowedGood;

        var name = id is null ? loc.T(NothingSelectedLocKey) : goods.GetGood(id).PluralDisplayName.Value;

        builder.Append(loc.T("LV.SC.Stockpile").Bold());
        builder.AppendLine(": " + name);

        var inv = component.Inventory;
        if (id is not null)
        {
            builder.AppendLine($"  {inv.TotalAmountInStock}/{inv.Capacity}");
        }
    }

}
