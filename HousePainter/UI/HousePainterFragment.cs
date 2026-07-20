namespace HousePainter.UI;

[BindFragment]
public class HousePainterFragment(
    ILoc t,
    IContainer container
) : BaseEntityPanelFragment<BuildingPartPainter>
{

#nullable disable
    Button btnEnable;

    VisualElement pnlPainting;
    VisualElement partsContainer;
#nullable enable

    protected override void InitializePanel()
    {
        btnEnable = panel.AddGameButtonPadded(t.T("LV.HPt.EnablePainting"), OnEnableRequested, stretched: true).SetDisplay(false);

        pnlPainting = panel.AddChild().SetDisplay(false);
        pnlPainting.AddEntityFragmentButton(t.T("LV.HPt.DisablePainting"), OnDisableRequested, color: EntityFragmentButtonColor.Red)
            .SetWidthPercent(100).SetMarginBottom();
        partsContainer = pnlPainting.AddChild();
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (!component) { return; }

        ReloadUI();
    }

    public override void ClearFragment()
    {
        if (component)
        {
            component!.ClearPartHighlights();
        }

        base.ClearFragment();
        btnEnable.SetDisplay(false);
        pnlPainting.SetDisplay(false);
        partsContainer.Clear();
    }

    void ReloadUI()
    {
        var enabled = component!.IsPaintingEnabled;
        btnEnable.SetDisplay(!enabled);
        pnlPainting.SetDisplay(enabled);

        partsContainer.Clear();

        if (!enabled) { return; }

        var paintedParts = component.PaintedParts;
        foreach (var p in component.PaintableParts)
        {
            var el = container.GetInstance<HousePaintingPartElement>();
            var name = p.MaterialName;

            el.OnLabelChanged += (_, e) => SetLabel(name, e);
            el.OnColorPicked += (_, e) => SetColor(name, e);
            el.OnHover += (_, e) => OnPartHover(name, e);

            el.SetOriginalLabel(name);

            if (paintedParts.TryGetValue(name, out var pp))
            {
                el.SetLabel(pp.Label ?? name);
                el.SetColor(pp.Color is { } c ? (Color)c : null);
            }
            else
            {
                el.SetLabel(name);
            }

            partsContainer.Add(el);
        }
    }

    void SetLabel(string name, string label)
    {
        if (!component) { return; }
        component!.SetLabel(name, label);
    }

    void SetColor(string name, Color? color)
    {
        if (!component) { return; }
        if (color.HasValue)
        {
            component!.ApplyPaint(name, color.Value);
        }
        else
        {
            component!.ClearPaint(name);
        }
    }

    void OnPartHover(string name, bool hover)
    {
        if (!component) { return; }
        component!.ToggleHighlightPart(name, hover);
    }

    void OnEnableRequested()
    {
        if (component is null) { return; }

        component.RequestPainting();
        ReloadUI();
    }

    void OnDisableRequested()
    {
        if (component is null) { return; }

        component.DisablePainting();
        ReloadUI();
    }

}
