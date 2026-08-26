namespace CraneHeads.UI;

public class TrebuchetStockRow : VisualElement
{
    readonly Label amount;

    public string GoodId { get; }

    public TrebuchetStockRow(IGoodService goods, string goodId)
    {
        GoodId = goodId;
        this.SetAsRow().AlignItems().SetMarginBottom(3);
        this.AddIconSpan().SetGood(goods, goodId, showName: true).SetFlexGrow(0).SetFlexShrink(0);
        this.AddChild().SetMarginLeftAuto();
        amount = this.AddGameLabel();
    }

    public void Set(int have, int need, int? perShot = null)
    {
        amount.text = perShot is { } shot
            ? $"{have} / {need} ({shot})"
            : $"{have} / {need}";
        amount.style.color = have >= need
            ? TimberUiUtils.SuccessColor
            : new StyleColor(StyleKeyword.Null);
    }
}
