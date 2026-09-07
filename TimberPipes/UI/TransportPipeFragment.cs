namespace TimberPipes.UI;

[BindFragment]
public class TransportPipeFragment(
    ILoc t,
    IGoodService goods,
    NamedIconProvider icons,
    PipeRegistry registry,
    PipeHighlighter highlighter,
    DialogService diag
) : BaseEntityPanelFragment<BuildingPipe>
{
    const float GaugeSize = 100f;

    static readonly Color GaugeBorder = new(0.55f, 0.6f, 0.65f, 0.9f);
    static readonly Color GaugeEmpty = new(0.12f, 0.14f, 0.16f, 0.55f);
    static readonly Color ContaminatedFill = new(0.42f, 0.32f, 0.14f, 0.95f);

    VisualElement fill = null!;
    IconSpan good = null!;
    Label amount = null!;
    Label headlift = null!;

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
        amount = stats.AddGameLabel().SetMarginBottom(5);
        headlift = stats.AddGameLabel();
        panel.AddGameButtonPadded(t.T("LV.TPi.Flush"), Flush, stretched: true).SetMargin(top: 8);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (!component)
        {
            return;
        }

        if (!component!.HasComponent<TransportPipeSpec>())
        {
            ClearFragment();
            return;
        }

        Refresh();
    }

    public override void ClearFragment()
    {
        highlighter.Unhighlight();
        base.ClearFragment();
    }

    public override void UpdateFragment()
    {
        if (!component)
        {
            return;
        }

        Refresh();
    }

    void Refresh()
    {
        if (component is not { } pipe)
        {
            return;
        }

        var capacity = BuildingPipe.MaxWaterHeight;
        var volume = Math.Clamp(pipe.FluidHeight, 0f, capacity);
        var ratio = capacity <= 0f ? 0f : volume / capacity;
        var contaminated = pipe.IsContaminated || pipe.Graph is { Contaminated: true };
        var extra = contaminated ? 0f : PipeHeadlift.ExtraAt(pipe, registry);

        fill.SetHeightPercent(ratio * 100f);
        fill.style.backgroundColor = GetFillColor(pipe);
        RefreshGood(pipe);
        amount.text = string.Format(
            t.T("LV.TPi.PipeFill"),
            volume,
            capacity);
        headlift.text = string.Format(
            t.T("LV.TPi.Headlift"),
            extra);
        HighlightGraph(pipe);
    }

    void HighlightGraph(BuildingPipe pipe)
    {
        if (pipe.Graph is { } graph)
        {
            highlighter.HighlightGraph(graph);
            return;
        }

        highlighter.Unhighlight();
    }

    async void Flush()
    {
        if (!await diag.ConfirmAsync("LV.TPi.FlushConfirm", localized: true))
        {
            return;
        }

        component?.Graph?.Flush();
        Refresh();
    }

    void RefreshGood(BuildingPipe pipe)
    {
        if (pipe.IsContaminated)
        {
            good.SetContent(icons.QuestionMark, postfixText: t.T("LV.TPi.Contaminated"), size: 24);
            return;
        }

        if (pipe.FluidGoodId is { } id && goods.HasGood(id))
        {
            good.SetGood(goods, id, showName: true);
            return;
        }

        good.SetContent(icons.QuestionMark, postfixText: t.TNone(), size: 24);
    }

    Color GetFillColor(BuildingPipe pipe)
    {
        if (pipe.IsContaminated)
        {
            return ContaminatedFill;
        }

        if (pipe.FluidGoodId is not { } id)
        {
            return GaugeEmpty;
        }

        return goods.GetGood(id).ContainerColor;
    }
}
