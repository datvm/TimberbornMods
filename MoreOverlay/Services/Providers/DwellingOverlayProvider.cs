namespace MoreOverlay.Services.Providers;


[MultiBind(typeof(IMoreOverlayProvider))]
public class DwellingOverlayProvider(NamedIconProvider namedIconProvider) : ComponentOverlayProviderBase<Dwelling, DwellingOverlayInstance>
{
    protected override DwellingOverlayInstance? CreateInstance(MoreOverlayComponent overlayComp, Dwelling comp)
        => new(overlayComp, comp, namedIconProvider);
}

public class DwellingOverlayInstance(MoreOverlayComponent overlayComp, Dwelling comp, NamedIconProvider namedIconProvider) : ComponentOverlayInstanceBase<Dwelling>(overlayComp, comp)
{
#nullable disable
    IconSpan adults, children, beds;
#nullable enable

    public override void Initialize(VisualElement container)
    {
        base.Initialize(container);

        adults = el.AddIconSpan(namedIconProvider.GetOrLoadGameIcon("ico-adult", "ico-adult"), size: IconSize).SetMarginRight(5);
        children = el.AddIconSpan(namedIconProvider.GetOrLoadGameIcon("ico-child", "ico-child"), size: IconSize).SetMarginRight(5);
        beds = container.AddIconSpan(namedIconProvider.GetOrLoadGameIcon("ico-bed", "ico-bed"), size: IconSize).SetMarginBottom(5);
    }
    public override void UpdateData()
    {
        var c = Component;
        adults.SetPostfixText(c.NumberOfAdultDwellers.ToString());
        children.SetPostfixText(c.NumberOfChildDwellers.ToString());
        beds.SetPostfixText($"{c.NumberOfDwellers}/{c.TotalSlots}");
    }

}