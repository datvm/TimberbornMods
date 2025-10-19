namespace ColorfulZipline.UI;

public class ColorfulZiplineFragment(
    ILoc t,
    ColorPickerShower colorPickerShower,
    EntityBadgeService entityBadgeService,
    DialogService diag,
    EntitySelectionService entitySelectionService,
    EntityRegistry entities
) : BaseEntityPanelFragment<ZiplineTowerColorComponent>
{
    VisualElement ziplinesContainer = null!;
    
    protected override void InitializePanel()
    {
        ziplinesContainer = panel.AddChild();
    }

    public override void ClearFragment()
    {
        base.ClearFragment();
        ziplinesContainer.Clear();
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);

        if (!component) { return; }

        var targets = component.ZiplineTower.ConnectionTargets;
        var count = targets.Count;

        if (count == 0)
        {
            ClearFragment();
            return;
        }

        for (int i = 0; i < count; i++)
        {
            var panel = new ColorfulZiplineConnectionPanel(t, colorPickerShower);
            panel.SetColor(component.GetColor(i));

            var target = targets[i];
            panel.SetTitle(t.T("LV.CZ.ConnectionLabel", entityBadgeService.GetEntityName(target)));

            var z = i;
            panel.OnColorSet += c => OnColorSet(z, c);
            panel.OnApplyBuildingRequested += OnApplyBuildingRequested;
            panel.OnApplyToAllRequested += OnApplyToAllRequested;

            ziplinesContainer.Add(panel);
        }
    }

    async void OnApplyToAllRequested(ZiplineCableColor c)
    {
        if (!await diag.ConfirmAsync(t.T("LV.CZ.ConfirmAll"))) { return; }

        foreach (var entity in entities.Entities)
        {
            var comp = entity.GetComponentFast<ZiplineTowerColorComponent>();
            if (comp)
            {
                comp.SetAllColor(c);
            }
        }

        Reselect();
    }

    void OnApplyBuildingRequested(ZiplineCableColor c)
    {
        if (component)
        {
            component.SetAllColor(c);
            Reselect();
        }
    }

    void OnColorSet(int index, ZiplineCableColor c)
    {
        if (!component) { return; }

        component.SetColor(index, c);
    }

    void Reselect()
    {
        var comp = component;
        if (!comp) { return; }

        entitySelectionService.Unselect();
        entitySelectionService.Select(comp);
    }
}
