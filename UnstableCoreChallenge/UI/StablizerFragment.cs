namespace UnstableCoreChallenge.UI;

public class StablizerFragment : BaseEntityPanelFragment<UnstableCoreStabilizer>
{
#nullable disable
    Label lblInfo;
#nullable enable

    protected override void InitializePanel()
    {
        lblInfo = panel.AddGameLabel();
    }

    public override void UpdateFragment()
    {
        if (!component) { return; }
    }

}
