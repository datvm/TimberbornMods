namespace TimberUi.Services.EntityPanelOrder;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class EntityFragmentOrderService(
    IEnumerable<IEntityFragmentOrder> orders,

#pragma warning disable CS9113 // For DI to ensure entityPanel is already loaded
    IEntityPanel entityPanel
#pragma warning restore CS9113 // Parameter is unread.
) : ILoadableSingleton
{

    public void Load()
    {
        var panels = orders.OrderBy(q => q.Order).ToArray();
        if (panels.Length == 0) { return; }

        VisualElement? prev = null;
        var prevNegative = true;

        foreach (var o in panels)
        {
            var panel = o.Fragment;
            var order = o.Order;

            if (o.Order < 0)
            {
                if (prev is null)
                {
                    panel.parent.Insert(0, panel);
                }
                else
                {
                    panel.InsertSelfAfter(prev);
                }
                prev = panel;
            }
            else if (o.Order > 0)
            {
                if (prev is null || prevNegative)
                {
                    prevNegative = false;
                    prev = panel.parent.Children().Last();
                }

                panel.InsertSelfAfter(prev);
                prev = panel;
            }
        }
    }
}
