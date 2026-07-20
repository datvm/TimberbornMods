using ModSettings.ColorPicker;

namespace HousePainter.UI;

[BindTransient]
public class HousePaintingPartElement : CollapsiblePanel
{
    readonly ILoc t;
    readonly DialogService diagServ;
    readonly ColorPickerShower colorPickerShower;
    readonly VisualElement colorDemo;
    readonly Button btnClearColor;
    readonly Label lblOriginal;

    Color? currColor;
    string originalLabel = "", currLabel = "";

    public event EventHandler<string> OnLabelChanged = null!;
    public event EventHandler<Color?> OnColorPicked = null!;
    public event EventHandler<bool> OnHover = null!;

    public HousePaintingPartElement(ILoc t, DialogService diagServ, ColorPickerShower colorPickerShower)
    {
        this.t = t;
        this.diagServ = diagServ;
        this.colorPickerShower = colorPickerShower;

        var header = HeaderLabel.parent;
        header.RegisterCallback<MouseEnterEvent>(_ => OnHover?.Invoke(this, true));
        header.RegisterCallback<MouseLeaveEvent>(_ => OnHover?.Invoke(this, false));

        colorDemo = this.AddChild().SetWidth(50).SetHeight(20).SetMargin(marginX: 10);
        colorDemo.InsertSelfAfter(HeaderLabel);

        var parent = Container;
        var row = parent.AddRow().AlignItems().SetMarginBottom();
        row.AddGameButtonPadded(t.T("LV.HPt.PickColor"), PickColor).SetFlexGrow();
        btnClearColor = row.AddGameButtonPadded(t.T("LV.HPt.ClearColor"), ClearColor).SetDisplay(false);

        parent.AddGameButtonPadded(t.T("LV.HPt.EditLabel"), EditName);
        lblOriginal = parent.AddLabel();

        SetExpand(false);
    }

    public void SetOriginalLabel(string label)
    {
        originalLabel = label;
        lblOriginal.text = t.T("LV.HPt.Original", label);
    }

    public void SetLabel(string name)
    {
        currLabel = name;
        SetTitle(currLabel);
    }

    public void SetColor(Color? color)
    {
        currColor = color;
        colorDemo.style.backgroundColor = color ?? Color.clear;
        btnClearColor.SetDisplay(color.HasValue);
    }

    async void EditName()
    {
        try
        {
            var name = await diagServ.PromptAsync(t.T("LV.HPt.EnterPartName", originalLabel), currLabel);

            if (name is null || name == currLabel) { return; }
            name = name.Trim();

            if (name.Length == 0)
            {
                name = originalLabel;
            }

            SetLabel(name);
            OnLabelChanged(this, currLabel);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    void PickColor()
    {
        colorPickerShower.ShowColorPicker(currColor ?? Color.white, true, c =>
        {
            SetColor(c);
            OnColorPicked(this, c);
        });
    }

    void ClearColor()
    {
        SetColor(null);
        OnColorPicked(this, null);
    }

}
