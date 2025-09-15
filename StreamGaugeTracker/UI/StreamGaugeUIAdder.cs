namespace StreamGaugeTracker.UI;

public class StreamGaugeUIAdder(
    IEntityPanel entityPanel,
    StreamGaugeTrackerFragment fragment,
    ILoc t
) : ILoadableSingleton
{

#nullable disable
    Label lblLowestDepth;
#nullable enable

    readonly EntityPanel entityPanel = (EntityPanel)entityPanel;

    public void Load()
    {
        var streamGaugePanel = entityPanel._root.Q("StreamGaugeFragment")
            ?? throw new Exception("StreamGaugeFragment not found in EntityPanel");

        var greatestDepth = streamGaugePanel.Q("GreatestDepthLabel")
            ?? throw new Exception("GreatestDepthLabel not found in StreamGaugeFragment");

        lblLowestDepth = streamGaugePanel.AddGameLabel(t.T("LV.SGT.LowestDepth", 0), name: "LowestDepthLabel");
        lblLowestDepth.InsertSelfAfter(greatestDepth);

        streamGaugePanel.Q<Button>("ResetGreatestDepthButton")
            .AddAction(fragment.OnResetRequested);

        fragment.UpdateLowestDepth += Fragment_UpdateLowestDepth;
    }

    private void Fragment_UpdateLowestDepth(float depth)
    {
        lblLowestDepth.text = t.T("LV.SGT.LowestDepth", depth == float.MaxValue ? 0 : depth);
    }

}
