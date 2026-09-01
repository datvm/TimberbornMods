namespace CraneHeads.UI;

[BindFragment]
public class WindmillFragment(ILoc t, WindService wind) : BaseEntityPanelFragment<CraneHeadWindmill>, IEntityFragmentOrder
{
    Label text = null!;

    public int Order => -50;
    public VisualElement Fragment => panel;

    CraneHeadWindmill? Shown => component is { } c && c ? c : null;

    protected override void InitializePanel()
        => text = panel.AddGameLabel();

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (Shown is not { } mill || !mill.Head.IsFinished)
        {
            panel.Visible = false;
            return;
        }

        panel.Visible = true;
        Refresh();
    }

    public override void UpdateFragment()
    {
        base.UpdateFragment();
        if (Shown is not { } mill || !mill.Head.IsFinished)
        {
            return;
        }

        Refresh();
    }

    void Refresh()
    {
        if (Shown is not { } mill)
        {
            return;
        }

        var generator = mill.Generator;
        var attached = generator is { HasHead: true };
        var generating = attached && wind.WindStrength > mill.Spec.MinRequiredWindStrength;
        var baseHp = attached ? generator!.BasePowerOutput : 0;
        var sections = attached ? generator!.SectionCount : 0;
        var fromWind = generating ? Mathf.CeilToInt(baseHp * wind.WindStrength) : 0;
        var fromHeight = generating ? Mathf.CeilToInt(fromWind * mill.Spec.BonusPerSection * sections) : 0;
        var output = attached ? generator!.CurrentPowerOutput : 0;

        text.text = string.Format(
            t.T("LV.CrH.WindmillOutput"),
            output,
            baseHp,
            Mathf.RoundToInt(wind.WindStrength * 100f),
            fromWind,
            fromWind,
            Mathf.RoundToInt(mill.Spec.BonusPerSection * sections * 100f),
            fromHeight);
    }
}
