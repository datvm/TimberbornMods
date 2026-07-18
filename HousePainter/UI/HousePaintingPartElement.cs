namespace HousePainter.UI;

[BindTransient]
public class HousePaintingPartElement : CollapsiblePanel
{
    readonly ILoc t;
    readonly DialogService diagServ;
    readonly VisualElement colorDemo;
    readonly Button btnClearColor;

    string originalLabel = "", currLabel = "";

    public event EventHandler<string> OnLabelChanged = null!;
    public event EventHandler<Color?> OnColorPicked = null!;

    public HousePaintingPartElement(ILoc t, DialogService diagServ)
    {
        this.t = t;
        this.diagServ = diagServ;

        var parent = Container;
        colorDemo = parent.AddChild().SetWidthPercent(100).SetHeight(20).SetMarginBottom();

        var row = parent.AddRow().AlignItems().SetMarginBottom();
        row.AddGameButtonPadded(t.T("LV.HPt.PickColor"), PickColor).SetFlexGrow();
        btnClearColor = row.AddGameButtonPadded(t.T("LV.HPt.ClearColor"), ClearColor).SetDisplay(false);

        parent.AddGameButtonPadded(t.T("LV.HPt.EditLabel"), EditName);

        SetExpand(false);
    }

    public void SetOriginalLabel(string label) => originalLabel = label;

    public void SetLabel(string name)
    {
        currLabel = name;
        SetTitle(currLabel);
    }

    public void SetColor(Color? color)
    {
        colorDemo.style.backgroundColor = color ?? Color.clear;
        btnClearColor.SetDisplay(color.HasValue);
    }

    async void EditName()
    {
        try
        {
            var name = await diagServ.PromptAsync(t.T("LV.HPt.EnterPartName", originalLabel), currLabel);

            if (string.IsNullOrWhiteSpace(name) || name == currLabel) return;

            SetLabel(name.Trim());
            OnLabelChanged(this, currLabel);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    async void PickColor()
    {
        try
        {
            // while waiting for Mod Settings update, enter manually
            var rgb = await diagServ.PromptAsync("Enter RGB color by comma:", "255,255,0");

            var parts = (rgb ?? "").Split(',');
            if (parts.Length != 3) { return; }

            var color = new Color(
                int.Parse(parts[0].Trim()) / 255f,
                int.Parse(parts[1].Trim()) / 255f,
                int.Parse(parts[2].Trim()) / 255f
            );
            SetColor(color);
            OnColorPicked(this, color);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    void ClearColor()
    {
        SetColor(null);
        OnColorPicked(this, null);
    }

}
