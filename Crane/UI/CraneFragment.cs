namespace Crane.UI;

[BindFragment]
public class CraneFragment(
    ILoc t,
    DialogService diag,
    CraneStructureService structureService
) : BaseEntityPanelFragment<CraneComponent>
{
#nullable disable
    Label height, range;
    Button buildHigher;
#nullable enable

    public CraneComponent? Crane => component;

    public event EventHandler<CraneComponent>? OnShowFragment;
    public event EventHandler<CraneComponent>? OnUpdateFragment;
    public event EventHandler? OnClearFragment;

    bool initialized;
    readonly List<Action<EntityPanelFragmentElement>> initializationActions = [];

    public void AppendInitializePanel(Action<EntityPanelFragmentElement> action)
    {
        if (initialized)
        {
            action.Invoke(panel);
        }
        else
        {
            initializationActions.Add(action);
        }
    }

    protected override void InitializePanel()
    {
        var row = panel.AddRow().AlignItems().SetMarginBottom();

        height = row.AddGameLabel();
        range = row.AddGameLabel();
        height.style.flexBasis = range.style.flexBasis = Length.Percent(50f);

        buildHigher = panel.AddGameButtonPadded(t.T("LV.Cr.BuildHigher"), BuildHigher).SetFlexGrow();

        initialized = true;
        foreach (var action in initializationActions)
        {
            action.Invoke(panel);
        }
        initializationActions.Clear();
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (!component || !component!.IsFinished)
        {
            panel.Visible = false;
            return;
        }

        buildHigher.SetEnabled(structureService.CanBuildHigher(component));
        OnShowFragment?.Invoke(this, component);
    }

    public override void UpdateFragment()
    {
        base.UpdateFragment();
        if (!component || !component!.IsFinished)
        {
            return;
        }

        UpdateData();
        OnUpdateFragment?.Invoke(this, component);
    }

    public override void ClearFragment()
    {
        OnClearFragment?.Invoke(this, EventArgs.Empty);
        base.ClearFragment();
    }

    void UpdateData()
    {
        var tower = component!.Tower;

        height.text = tower.TargetHeight > tower.Height
            ? t.T("LV.Cr.HeightPending", tower.Height, tower.TargetHeight)
            : t.T("LV.Cr.Height", tower.Height);
        range.text = t.T("LV.Cr.Range", tower.HorizontalRange);
    }

    void BuildHigher()
    {
        if (!component) { return; }

        if (!structureService.TryBuildHigher(component!))
        {
            diag.Alert("LV.Cr.CannotBuildHigher", true);
            buildHigher.SetEnabled(false);
            return;
        }

        UpdateData();
        buildHigher.SetEnabled(structureService.CanBuildHigher(component!));
    }

}
