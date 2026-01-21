namespace TImprove4Ui.Services;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class ScrollableEntityPanelService(
    IEntityPanel entityPanel,
    VisualElementInitializer veInit
) : ILoadableSingleton
{

    readonly EntityPanel entityPanel = entityPanel as EntityPanel
        ?? throw new InvalidDataException($"{nameof(IEntityPanel)} is not {nameof(EntityPanel)}");

    public void Load()
    {
        if (!MSettings.ScrollableEntityPanel) { return; }

        var root = entityPanel._root;
        AddScroll(root);
        AddStockpileInventoryScroll(root);
    }

    void AddScroll(VisualElement root)
    {
        root.style.maxHeight = new Length(80, LengthUnit.Percent);

        var stockpileInventory = root.Q("StockpileInventoryFragmentWrapper");
        var fragments = root.Q("Fragments");
        var parent = fragments.parent;

        var scrollContainer = new VisualElement();
        var scroll = scrollContainer
            .AddScrollView("FragmentScroll", greenDecorated: false, additionalClasses: ["game-scroll-view"])
            .Initialize(veInit);

        var index = -1;
        var curr = 0;
        foreach (var child in parent.Children())
        {
            if (child == fragments)
            {
                index = curr;
            }
            else
            {
                child.SetFlexShrink(0);
            }

            curr++;
        }

        scroll.Add(fragments);
        parent.Insert(index, scrollContainer);

        // Move the Good selection panel out of the scroll to prevent issue
        if (stockpileInventory is not null)
        {
            stockpileInventory.SetFlexShrink(0);
            stockpileInventory.InsertSelfBefore(scroll);
        }
    }

    void AddStockpileInventoryScroll(VisualElement root)
    {
        var el = root.Q("GoodSelection");
        if (el is null) { return; }

        var scrollView = root
            .AddScrollView(greenDecorated: false, additionalClasses: ["game-scroll-view"])
            .Initialize(veInit)
            .SetMaxHeight(400);

        scrollView.InsertSelfAfter(el);
        scrollView.Add(el);
    }

}
