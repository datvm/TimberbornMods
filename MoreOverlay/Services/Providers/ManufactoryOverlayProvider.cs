namespace MoreOverlay.Services.Providers;

[MultiBind(typeof(IMoreOverlayProvider))]
public class ManufactoryOverlayProvider(NamedIconProvider namedIconProvider, ILoc t, MoreOverlayHighlighter highlighter) : ComponentOverlayProviderBase<Manufactory, ManufactoryOverlayInstance>
{
    protected override ManufactoryOverlayInstance CreateInstance(MoreOverlayComponent overlayComp, Manufactory comp)
        => new(overlayComp, comp, namedIconProvider, t, highlighter);
}

public class ManufactoryOverlayInstance(MoreOverlayComponent overlayComp, Manufactory comp, NamedIconProvider namedIconProvider, ILoc t, MoreOverlayHighlighter highlighter) : ComponentOverlayInstanceBase<Manufactory>(overlayComp, comp), ITickableMoreOverlayInstance
{

    readonly WorkshopProductivityCounter? productivityComp = comp.GetComponent<WorkshopProductivityCounter>();

#nullable disable
    IconSpan recipe;
    IconSpan productivity;
#nullable enable

    public override void Initialize(VisualElement container)
    {
        base.Initialize(container);
        recipe = el.AddIconSpan().SetImageSize(IconSize).SetMarginRight(5);

        if (productivityComp)
        {
            productivity = container
                .AddIconSpan(namedIconProvider.GetOrLoadGameIcon("ico-work-empty-beaver", "ico-work-empty-beaver"), size: IconSize)
                .SetMarginBottom(5);
        }

        Component.RecipeChanged += (_, _) => UpdateData();
    }

    public void OnTickUpdate() => UpdateProductivity();

    void UpdateProductivity()
    {
        if (!productivityComp) { return; }

        var prod = productivityComp!.CalculateProductivity();
        productivity.SetPostfixText(prod.ToString("P0"));
        highlighter.Highlight(OverlayComponent, prod, 100);
    }

    public override void UpdateData()
    {
        Sprite icon; string name;
        if (Component.CurrentRecipe is { } r)
        {
            icon = r.Icon?.Asset ?? namedIconProvider.QuestionMark;
            name = t.T(r.DisplayLocKey);
        }
        else
        {
            icon = namedIconProvider.QuestionMark;
            name = t.TNone();
        }

        recipe.SetContent(icon, null, name);
        UpdateProductivity();
    }

}