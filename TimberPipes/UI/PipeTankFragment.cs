namespace TimberPipes.UI;

[BindFragment]
public class PipeTankFragment(
    ILoc t,
    IGoodService goods,
    NamedIconProvider icons
) : BaseEntityPanelFragment<PipeTank>
{
    const float GaugeSize = 100f;

    static readonly Color GaugeBorder = new(0.55f, 0.6f, 0.65f, 0.9f);
    static readonly Color GaugeEmpty = new(0.12f, 0.14f, 0.16f, 0.55f);

    VisualElement fill = null!;
    IconSpan good = null!;
    Label volume = null!;
    Label level = null!;

    protected override void InitializePanel()
    {
        var row = panel.AddRow().AlignItems();

        var gauge = row.AddChild()
            .SetSize(GaugeSize)
            .SetMarginRight(12)
            .SetBorder(GaugeBorder, 2);
        var gs = gauge.style;
        gs.overflow = Overflow.Hidden;
        gs.backgroundColor = GaugeEmpty;
        gs.flexDirection = FlexDirection.Column;
        gs.justifyContent = Justify.FlexEnd;
        gs.borderTopLeftRadius = gs.borderTopRightRadius = gs.borderBottomLeftRadius = gs.borderBottomRightRadius
            = Length.Percent(50);

        fill = gauge.AddChild().SetWidthPercent(100);

        var stats = row.AddChild();
        good = stats.AddIconSpan().SetMarginBottom(5);
        volume = stats.AddGameLabel().SetMarginBottom(5);
        level = stats.AddGameLabel();
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (component is not { Enabled: true })
        {
            ClearFragment();
            return;
        }

        Refresh();
    }

    public override void UpdateFragment()
    {
        if (component is not { Enabled: true })
        {
            return;
        }

        Refresh();
    }

    void Refresh()
    {
        if (component is not { } tank)
        {
            return;
        }

        var capacity = tank.CapacityM3;
        var amount = Math.Max(0f, tank.VolumeM3);
        var ratio = capacity <= 0f ? 0f : Math.Clamp(amount / capacity, 0f, 1f);
        fill.SetHeightPercent(ratio * 100f);
        fill.style.backgroundColor = GetFillColor(tank);
        RefreshGood(tank);
        volume.text = string.Format(t.T("LV.TPi.PipeFill"), amount, capacity);
        level.text = string.Format(t.T("LV.TPi.TankLevel"), tank.FillHeight, (float)tank.HeightTiles);
    }

    void RefreshGood(PipeTank tank)
    {
        if (tank.FluidGoodId is { } id && goods.HasGood(id))
        {
            good.SetGood(goods, id, showName: true);
            return;
        }

        good.SetContent(icons.QuestionMark, postfixText: t.TNone(), size: 24);
    }

    Color GetFillColor(PipeTank tank)
    {
        if (tank.FluidGoodId is not { } id || !goods.HasGood(id))
        {
            return GaugeEmpty;
        }

        return goods.GetGood(id).ContainerColor;
    }
}
