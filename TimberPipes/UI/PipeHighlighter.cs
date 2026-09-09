namespace TimberPipes.UI;

[BindSingleton]
public class PipeHighlighter(Highlighter highlighter)
{
    static readonly Color EmptyColor = TimberUiUtils.DangerColor;
    static readonly Color FullColor = TimberUiUtils.SuccessColor;

    public void HighlightGraph(PipeGraph graph, BuildingPipe? selected)
    {
        Unhighlight();
        foreach (var pipe in graph.Pipes.Values)
        {
            if (!pipe || pipe == selected)
            {
                continue;
            }

            highlighter.HighlightPrimary(pipe, ColorFor(pipe));
        }
    }

    public void Unhighlight() => highlighter.UnhighlightAllPrimary();

    static Color ColorFor(BuildingPipe pipe)
    {
        var cap = BuildingPipe.MaxWaterHeight;
        var t = cap <= 0f ? 0f : Math.Clamp(pipe.FluidHeight / cap, 0f, 1f);
        return Color.Lerp(EmptyColor, FullColor, t);
    }
}
