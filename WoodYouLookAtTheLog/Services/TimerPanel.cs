namespace WoodYouLookAtTheLog.Services;

[BindSingleton]
public class TimerPanel(
    UILayout uiLayout,
    ILoc t
) : VisualElement, ILoadableSingleton
{
    Label lbl = null!;

    public void Load()
    {
        this.SetPadding(10).SetDisplay(false);

        var row = this.AddRow().AlignItems();
        row.AddLabel(t.T("LV.WYLATL.GameClosesIn")).SetMarginRight(5);
        lbl = row.AddLabel();

        uiLayout.AddBottomRight(this, 1);
    }

    public void Show() => this.SetDisplay(true);
    public void Hide() => this.SetDisplay(false);
    public void SetTime(int minutes) => lbl.text = minutes.ToString();

}
