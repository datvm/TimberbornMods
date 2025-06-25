namespace HydroFormaProjects.UI;

public class SluiceUpstreamFragment(
    ILoc t,
    SluiceUpstreamService sluiceService,
    MapSize mapSize
) : ILoadableSingleton, IUnloadableSingleton
{

    public static SluiceUpstreamFragment? Instance { get; private set; }
    public static SluiceFragment? PendingFragment { get; set; }

#nullable disable
    VisualElement upstreamPanel;
    Toggle chkUpstream;

    VisualElement upstreamThresholdPanel;
    FloatField txtUpstream;

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
        Debug.Log($"Initializing {nameof(SluiceUpstreamFragment)}");

        sluiceFragment = instance;
        upstreamPanel = new();
        upstreamPanel.SetDisplay(false);

        chkUpstream = upstreamPanel.AddToggle(t.T("LV.HF.SluiceOpenUpstream"), onValueChanged: SetAutoOpen);

        upstreamThresholdPanel = upstreamPanel.AddRow().AlignItems();
        txtUpstream = upstreamThresholdPanel.AddFloatField(changeCallback: SetThresholdValue).SetFlexGrow().SetMarginRight(10);
        upstreamThresholdPanel.AddMinusButton().AddAction(() => SetThresholdDelta(-.05f));
        upstreamThresholdPanel.AddPlusButton().AddAction(() => SetThresholdDelta(.05f));
    }

    public void ShowFragment()
    {
        if (!sluiceFragment._sluice || !sluiceService.CanUseSluiceUpstream) { return; }

        comp = sluiceFragment._sluice.GetComponentFast<SluiceUpstreamComponent>();
        if (!comp)
        {
            comp = null;
            return;
        }

        UpdateContentPanel();
        upstreamPanel.SetDisplay(true);
    }

    public void ClearFragment()
    {
        upstreamPanel.SetDisplay(false);
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
    }

    void SetThresholdDelta(float delta)
    {
        if (!comp) { return; }

        SetThresholdValue(comp.Threshold + delta);
    }

    void SetThresholdValue(float value)
    {
        if (!comp) { return; }

        value = RoundToNearest05(Mathf.Clamp(value, 0f, mapSize.TerrainSize.z));
        
    }

    void SetAutoOpen(bool enabled)
    {
        if (!comp) { return; }
        comp.AutoOpen = enabled;
    }

    static float RoundToNearest05(float value) => Mathf.Round(value / 0.05f) * 0.05f;

}
