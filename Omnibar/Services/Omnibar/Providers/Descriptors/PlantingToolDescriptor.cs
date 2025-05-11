namespace Omnibar.Services.Omnibar.Providers.Descriptors;

public class PlantingToolDescriptor(IContainer container, GrowableSpec growable) : IOmnibarDescriptor
{

    readonly GatherableSpec gatherable = growable.GetComponentFast<GatherableSpec>();

    public bool Describe(VisualElement el)
    {
        var fac = container.GetInstance<GrowableToolPanelItemFactory>();
        el.Add(RemoveInfoIcon(fac.Create(growable)));

        if (gatherable)
        {
            var fac2 = container.GetInstance<GatherableToolPanelItemFactory>();
            el.Add(RemoveInfoIcon(fac2.Create(gatherable))
                .SetMargin(left: 20));
        }

        return true;
    }

    static VisualElement RemoveInfoIcon(VisualElement el)
    {
        var icon = el.Q("InfoIcon");
        icon?.parent.Remove(icon);

        return el;
    }

}
