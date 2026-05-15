namespace MoreOverlay.Services.Providers;

[MultiBind(typeof(IMoreOverlayProvider))]
public class GrowableOverlayProvider(MoreOverlayHighlighter highlighter) : ComponentOverlayProviderBase<Growable, GrowableOverlayInstance>
{
    protected override GrowableOverlayInstance? CreateInstance(MoreOverlayComponent overlayComp, Growable comp)
        => new(overlayComp, comp, highlighter);
}

public class GrowableOverlayInstance(MoreOverlayComponent overlayComp, Growable comp, MoreOverlayHighlighter highlighter) : ComponentOverlayInstanceBase<Growable>(overlayComp, comp)
{
    readonly LivingNaturalResource livingNaturalResource = comp.GetComponent<LivingNaturalResource>();
    readonly Cuttable? cuttable = comp.GetComponent<Cuttable>();

    bool dead;

    public override void Initialize(VisualElement container)
    {
        base.Initialize(container);


    }

    public override void UpdateData()
    {

    }
}
