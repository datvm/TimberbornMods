namespace MoreOverlay.Services.Providers;

[MultiBind(typeof(IMoreOverlayProvider))]
public class WaterSourceOverlayProvider(NamedIconProvider namedIconProvider, MoreOverlayHighlighter highlighter) : ComponentOverlayProviderBase<WaterSource, WaterSourceOverlayInstance>
{
    protected override WaterSourceOverlayInstance CreateInstance(MoreOverlayComponent overlayComp, WaterSource comp)
        => new(overlayComp, comp, namedIconProvider, highlighter);
}

public class WaterSourceOverlayInstance(MoreOverlayComponent overlayComp, WaterSource comp, NamedIconProvider namedIconProvider, MoreOverlayHighlighter highlighter) : ComponentOverlayInstanceBase<WaterSource>(overlayComp, comp), ITickableMoreOverlayInstance
{
    readonly WaterSourceContamination contaminationComp = comp.GetComponent<WaterSourceContamination>();

#nullable disable
    IconSpan strength, contamination;
#nullable enable

    public override void Initialize(VisualElement container)
    {
        base.Initialize(container);

        strength = el.AddIconSpan(namedIconProvider.Water, size: IconSize).SetMarginRight(10);
        contamination = el.AddIconSpan(namedIconProvider.GetOrLoadTopbar("badwater", "Badwater"), size: IconSize);
    }

    public override void OnShow() => UpdateData();
    public void OnTickUpdate() => UpdateData();

    public override void UpdateData()
    {
        strength.SetPostfixText(Component.CurrentStrength.ToString("F1"));

        var c = contaminationComp.Contamination;
        contamination.SetPostfixText(c.ToString("P0"));

        highlighter.Highlight(OverlayComponent, 1 - c, 100);
    }

}