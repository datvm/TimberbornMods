namespace HydroFormaProjects.UI;

public class SluiceUpstreamFragment(
    ILoc t,
    SluiceUpstreamService sluiceUpstreamService,
    VisualElementInitializer veInit
) : ILoadableSingleton, IUnloadableSingleton
{
    const float HeightStep = .05f;

    public static SluiceUpstreamFragment? Instance { get; private set; }
    public static SluiceFragment? PendingFragment { get; set; }

#nullable disable
    VisualElement upstreamPanel;
    Toggle chkUpstream;

    VisualElement upstreamThresholdPanel;
    FloatField txtUpstream;
    WaterHeightSlider sldUpstream;

    VisualElement upstreamHeightPanel;
    Label lblUpstream;

    SluiceFragment sluiceFragment;
#nullable enable

    SluiceUpstreamComponent? comp;

    public void Load()
    {
        Instance = this;

        if (PendingFragment is not null)
        {
            sluiceFragment = PendingFragment;
            PendingFragment = null;
            InitializeFragment(sluiceFragment);
        }
    }

    public void InitializeFragment(SluiceFragment instance)
    {
        sluiceFragment = instance;
        upstreamPanel = new();
        upstreamPanel.SetDisplay(false);

        chkUpstream = upstreamPanel.AddToggle(t.T("LV.HF.SluiceOpenUpstream"), onValueChanged: SetAutoOpen,
            additionalClasses: ["game-toggle", "entity-panel__text", "entity-panel__toggle"])
            .SetWidthPercent(100);        
        chkUpstream.classList.Remove("settings-toggle");
        chkUpstream.classList.Remove("settings-text");

        upstreamThresholdPanel = upstreamPanel.AddRow().AlignItems();
        txtUpstream = upstreamThresholdPanel.AddFloatField(changeCallback: SetThresholdValue).SetFlexGrow().SetMarginRight(10);
        upstreamThresholdPanel.AddMinusButton(size: UiBuilder.GameButtonSize.Small).AddAction(() => SetThresholdDelta(-HeightStep));
        upstreamThresholdPanel.AddPlusButton(size: UiBuilder.GameButtonSize.Small).AddAction(() => SetThresholdDelta(HeightStep));

        sldUpstream = upstreamPanel.AddChild<WaterHeightSlider>()
            .SetLabelVisible(false)
            .RegisterHeightChange(SetThresholdValue);

        upstreamHeightPanel = upstreamPanel.AddRow().SetDisplay(false);
        upstreamHeightPanel.AddGameLabel(t.T("LV.HF.SluiceUpstreamDepth")).SetMarginRight(5);
        lblUpstream = upstreamHeightPanel.AddGameLabel("0.00", name: "UpstreamDepth");
        upstreamHeightPanel.InsertSelfBefore(instance._root.Q("DepthLabel"));

        instance._root.Q("Automation").Add(
            upstreamPanel.Initialize(veInit)
        );
    }

    public void ShowFragment()
    {
        if (!sluiceFragment._sluice || !sluiceUpstreamService.CanUseProject) { return; }

        comp = sluiceFragment._sluice.GetComponentFast<SluiceUpstreamComponent>();
        if (!comp)
        {
            comp = null;
            return;
        }

        UpdateContentPanel();

        upstreamHeightPanel.SetDisplay(true);
        upstreamPanel.SetDisplay(true);
    }

    public void UpdateFragment()
    {
        if (!comp || !sluiceFragment._sluice) { return; }

        lblUpstream.text = sluiceFragment._sluice._threadSafeWaterMap.WaterDepth(comp.ThresholdCoordinates).ToString("0.00");
    }

    public void ClearFragment()
    {
        upstreamPanel.SetDisplay(false);
        upstreamHeightPanel.SetDisplay(false);
        comp = null;
    }

    public void Unload()
    {
        Instance = null;
        PendingFragment = null;
    }

    void UpdateContentPanel()
    {
        chkUpstream.SetValueWithoutNotify(comp!.AutoOpen);

        txtUpstream.SetValueWithoutNotify(comp.Threshold);
        sldUpstream.SetValues(comp.Threshold, comp.MaxThreshold);
    }

    void SetThresholdDelta(float delta)
    {
        if (!comp) { return; }

        SetThresholdValue(comp.Threshold + delta);
    }

    void SetThresholdValue(float value)
    {
        if (!comp) { return; }

        var h = comp.Threshold = RoundToNearest05(Mathf.Clamp(value, 0f, comp.MaxThreshold));
        SyncSluice();

        txtUpstream.SetValueWithoutNotify(h);
        sldUpstream.Height = h;
    }

    void SetAutoOpen(bool enabled)
    {
        if (!comp) { return; }

        comp.AutoOpen = enabled;
        SyncSluice();
    }

    static float RoundToNearest05(float value) => Mathf.Round(value / HeightStep) * HeightStep;

    void SyncSluice()
    {
        sluiceUpstreamService.SyncSluice(sluiceFragment._sluiceState);
    }

}
