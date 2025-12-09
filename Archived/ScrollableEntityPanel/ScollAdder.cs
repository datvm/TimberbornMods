namespace ScrollableEntityPanel;
public class ScollAdder(IEntityPanel entityPanel, VisualElementInitializer veInit) : ILoadableSingleton
{
    readonly EntityPanel entityPanel = entityPanel as EntityPanel
        ?? throw new InvalidDataException($"{nameof(IEntityPanel)} is not {nameof(EntityPanel)}");

    public void Load()
    {
        var root = entityPanel._root;
        root.style.maxHeight = new Length(80, LengthUnit.Percent);

        var stockpileInventory = root.Q("StockpileInventoryFragmentWrapper");
        var fragments = root.Q("Fragments");
        var parent = fragments.parent;

        var scrollContainer = new VisualElement();

        var scroll = new ScrollView()
        {
            name = "FragmentScroll"
        };
        scroll.classList.Add("game-scroll-view");
        veInit.InitializeVisualElement(scroll);


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
                child.style.flexShrink = 0;
            }

            curr++;
        }

        scroll.Add(fragments);
        parent.Insert(index, scrollContainer);

        // Move the Good selection panel out of the scroll to prevent issue
        if (stockpileInventory is not null)
        {
            stockpileInventory.style.flexShrink = 0;
            scrollContainer.Add(stockpileInventory);
        }
        
        scrollContainer.Add(scroll);

    }
}
