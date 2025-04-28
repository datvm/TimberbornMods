namespace TImprove4Ui.Services;

public class PowerNetworkHighlighter(
    EventBus eb,
    ISpecService spec,
    Highlighter highlighter
) : ILoadableSingleton
{

    Color color;

    public void Load()
    {
        eb.Register(this);
        color = spec.GetSingleSpec<ClusterHighlighterSpec>().ClusterSelection;
    }

    [OnEvent]
    public void OnObjectSelected(SelectableObjectSelectedEvent ev)
    {
        if (!MSettings.HighlightPowerNetwork) { return; }

        var mechNode = ev.SelectableObject.GetComponentFast<MechanicalNode>();
        if (!mechNode || !mechNode.enabled || mechNode.Graph is null) { return; }

        foreach (var node in mechNode.Graph.Nodes)
        {
            highlighter.HighlightSecondary(node, color);
        }
    }

    [OnEvent]
    public void OnObjectDeselected(SelectableObjectUnselectedEvent _)
    {
        highlighter.UnhighlightAllSecondary();
    }

}
