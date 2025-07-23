namespace BenchmarkAndOptimizer.UI;

public class BenchmarkDialog : DialogBoxElement
{
    const int DurationStep = 5;

    private readonly PanelStack ps;
    private readonly ILoc t;
    private readonly VisualElementInitializer veInit;
    private readonly GameBenchmarkService bmService;
    readonly VisualElement container;

#nullable disable
    GameSliderInt txtDuration;
#nullable enable

    public BenchmarkDialog(PanelStack ps, ILoc t, VisualElementInitializer veInit, GameBenchmarkService bmService)
    {
        this.ps = ps;
        this.t = t;
        this.veInit = veInit;
        this.bmService = bmService;

        SetTitle(t.T("LV.BO.Benchmark"));
        AddCloseButton();

        container = Content.AddChild();
        this.Initialize(veInit);
    }

    public void ReloadContent()
    {
        container.Clear();

        AddBenchmarkPanel(container);


        container.Initialize(veInit);
    }

    void AddBenchmarkPanel(VisualElement parent)
    {
        if (bmService.IsBenchmarking)
        {
            parent.AddGameLabel(t.T("LV.BO.BenchmarkRunning", bmService.RemainingTime.ToString("F2")));
            parent.AddMenuButton(t.T("LV.BO.EndBm"), onClick: StopBm, stretched: true);

            return;
        }

        AddBenchmarkResult(parent);

        parent.AddGameLabel(t.T("LV.BO.BmTime"));
        txtDuration = parent.AddSliderInt(values: new(1, 300 / DurationStep, 30 / DurationStep))
            .AddEndLabel(v => $"{v * 5:00}s")
            .SetMarginBottom();

        parent.AddGameLabel(t.T("LV.BO.BmInfo")).SetMarginBottom();
        parent.AddMenuButton(t.T("LV.BO.StartBm"), onClick: StartBm, size: UiBuilder.GameButtonSize.Large, stretched: true);
    }

    void AddBenchmarkResult(VisualElement parent)
    {
        var result = bmService.Result;
        if (result?.Summary is null) { return; }

        parent = parent.AddChild().SetPadding(bottom: 20).SetMarginBottom();
        var s = parent.style;
        s.borderBottomColor = Color.white;
        s.borderBottomWidth = 1;

        var scroll = parent.AddScrollView()
            .SetMaxHeight(500);

        scroll.AddChild();

        scroll.AddChild<SummaryElement>()
            .Init(["Type", "Total", "Average", "Count", "Min", "Max"]);

        foreach (var item in result.Summary.Value)
        {
            scroll.AddChild<SummaryElement>()
                .Init([item.Type.Name, F(item.Total), F(item.Average), item.SampleCount.ToString(), F(item.Min), F(item.Max)]);
        }
    }

    static string F(float value) => value.ToString("F2");

    void StartBm()
    {
        OnUIConfirmed();
        bmService.StartBenchmarking(txtDuration.Value * DurationStep);
    }

    void StopBm() => bmService.EndBenchmarking();

    public async Task<bool> ShowAsync()
    {
        ReloadContent();
        return await InternalShowAsync();
    }

    async Task<bool> InternalShowAsync()
    {
        return await ShowAsync(null, ps);
    }

}

public class SummaryElement : VisualElement
{

    public SummaryElement Init(string[] values)
    {
        this.SetAsRow().AlignItems().SetMarginBottom(5);

        for (int i = 0; i < values.Length; i++)
        {
            var container = this.AddChild();
            container.AddGameLabel(values[i]).SetWidthPercent(100);

            if (i == 0)
            {
                container.SetFlexGrow();
            }
            else
            {
                container
                    .SetFlexShrink(0)
                    .SetFlexGrow(0)
                    .style.flexBasis = new(new Length(10, LengthUnit.Percent));
            }
        }

        return this;
    }

}