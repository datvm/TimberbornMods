namespace MoreOverlay.Services;

[BindSingleton]
public class MoreOverlayHighlighter(Highlighter highlighter)
{
    static readonly float r0 = TimberUiUtils.DangerColor.r;
    static readonly float g0 = TimberUiUtils.DangerColor.g;
    static readonly float b0 = TimberUiUtils.DangerColor.b;
    static readonly float r1 = TimberUiUtils.SuccessColor.r;
    static readonly float g1 = TimberUiUtils.SuccessColor.g;
    static readonly float b1 = TimberUiUtils.SuccessColor.b;

    public void Highlight(MoreOverlayComponent comp, float perc, int priority) => PerformHighlight(comp,
        () => new Color(
            Mathf.Lerp(r0, r1, perc),
            Mathf.Lerp(g0, g1, perc),
            Mathf.Lerp(b0, b1, perc)),
        priority);

    public void Highlight(MoreOverlayComponent comp, Color color, int priority)
        => PerformHighlight(comp, () => color, priority);

    void PerformHighlight(MoreOverlayComponent comp, Func<Color> colorFn, int priority)
    {
        if (comp.ColorPriority > priority) { return; }

        comp.ColorPriority = priority;
        highlighter.HighlightPrimary(comp, colorFn());
    }

    public void UnhighlightAll()
    {
        highlighter.UnhighlightAllPrimary();
    }

}
