using LoadRate = Timberborn.AttractionsUI.AttractionLoadRateFragment.LoadRate;

namespace StreamGaugeTracker.UI;

public class StreamGaugeTrackerFragment(
    VisualElementLoader veLoader,
    MSettings s,
    ILoc t,
    ITooltipRegistrar tooltipRegistrar,
    IAssetLoader assetLoader,
    IDayNightCycle dayNightCycle
) : BaseEntityPanelFragment<StreamGaugeTrackerComponent>
{
    public event Action<float> UpdateLowestDepth = null!;

#nullable disable
    VisualElement ratesContainer;
    Label lblHighest;

    Texture2D bgGreen, bgRed;
#nullable enable
    ImmutableArray<LoadRate> bars = [];

    public int SamplesPerBar => Math.Max(1, s.SamplesPerBar.Value);

    protected override void InitializePanel()
    {
        bgGreen = assetLoader.Load<Texture2D>("UI/Images/Game/needview-background");
        bgRed = assetLoader.Load<Texture2D>("UI/Images/Game/needview-background-red");

        s.SamplesPerBar.ValueChanged += SamplesPerBar_ValueChanged;
        s.SamplingCount.ValueChanged += SamplingCount_ValueChanged;

        panel.AddGameLabel(t.T("Name_StreamGaugeTrackerFragment")).SetMarginBottom(5);
        lblHighest = panel.AddGameLabel(t.T("LV.SGT.DataHighest", 0), name: "HistoryDataHighest").SetMarginBottom(5);

        var ratePanel = veLoader.LoadVisualElement("Game/EntityPanel/AttractionLoadRateFragment");
        ratesContainer = ratePanel.Q("LoadRates");
        panel.Add(ratesContainer);

        ratesContainer.style.flexWrap = Wrap.Wrap;
        LoadRateBars();
    }

    private void SamplingCount_ValueChanged(object sender, int e)
    {
        if (component)
        {
            component.EnsureSize();
        }

        LoadRateBars();
    }

    private void SamplesPerBar_ValueChanged(object sender, int e) => LoadRateBars();

    void LoadRateBars()
    {
        ratesContainer.Clear();

        var samplesPerBar = SamplesPerBar;
        var barCount = (s.SamplingCount.Value + samplesPerBar - 1) / samplesPerBar;
        
        List<LoadRate> list = [];
        for (int i = 0; i < barCount; i++)
        {
            var el = veLoader.LoadVisualElement("Game/AttractionLoadRate");
            ratesContainer.Add(el);

            list.Add(new(
                el.Q("Rate"),
                el.Q("CurrentHourMarker")
            ));

            var z = i;
            tooltipRegistrar.Register(el, () => CreateTooltipContent(z));
        }

        bars = [.. list];

        UpdateFragment(); // If needed
    }

    TooltipContent CreateTooltipContent(int i)
    {
        if (!component) { return TooltipContent.CreateInstant(""); }

        StringBuilder content = new();
                
        var samplesPerBar = SamplesPerBar;
        var data = component.DepthHistory
            .Skip(i * samplesPerBar)
            .Take(samplesPerBar)
            .ToList();

        if (data.Count == 0)
        {
            content.Append(t.T("LV.SGT.TooltipNoData"));
        }
        else
        {
            var time = dayNightCycle.PartialDayNumber;
            var total = 0f;

            foreach (var item in data)
            {
                content.AppendLine(t.T("LV.SGT.TooltipLine", time - item.Time, item.Depth));
                total += item.Depth;
            }

            var average = total / data.Count;
            content.AppendLine(t.T("LV.SGT.TooltipAvg", average));
        }

        return TooltipContent.CreateInstant(content.ToString());
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (!component) { return; }
        UpdateFragment();
    }

    public override void UpdateFragment()
    {
        base.UpdateFragment();
        if (!component) { return; }

        UpdateLowestDepth(component.LowestDepth);
        UpdateRates();
    }

    public void OnResetRequested()
    {
        if (!component) { return; }
        component.ResetLowestValue();
        UpdateLowestDepth(component.LowestDepth);
    }

    void UpdateRates()
    {
        var barCount = bars.Length;

        var history = component!.DepthHistory.ToArray();
        var dataCount = history.Length;

        var samplePerBar = SamplesPerBar;
        var highest = component.HighestHistoryDepth;
        lblHighest.text = t.T("LV.SGT.DataHighest", highest);

        var finalBarIndex = (dataCount - 1) / samplePerBar;

        for (int i = 0; i < barCount; i++)
        {
            var startIndex = i * samplePerBar;
            var bar = bars[i];
            bar.CurrentHourMarker.SetDisplay(i == finalBarIndex);
            var hasHazardous = false;

            if (startIndex >= dataCount)
            {
                bar.Rate.style.height = 0;
            }
            else
            {
                bar = bars[i];

                var endIndex = Math.Min(startIndex + samplePerBar, dataCount);
                var total = 0f;

                for (int j = startIndex; j < endIndex; j++)
                {
                    var entry = history[j];

                    total += entry.Depth;
                    if (entry.IsHazardousWeather) { hasHazardous = true; }
                }

                bar.Rate.style.height = new(Length.Percent(highest == 0 ? 0 : (total / (endIndex - startIndex) / highest * 100f)));
            }

            bar.Rate.style.backgroundImage = hasHazardous ? bgRed : bgGreen;
        }
    }

}
