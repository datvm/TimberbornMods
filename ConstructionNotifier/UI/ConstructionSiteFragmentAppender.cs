namespace ConstructionNotifier.UI;

public class ConstructionSiteFragmentAppender(
#pragma warning disable CS9113 // For DI
    IEntityPanel _,
#pragma warning restore CS9113 // Parameter is unread.
    ILoc t,
    ConstructionSiteFragment fragment,
    EventBus eb
) : ILoadableSingleton
{

#nullable disable
    Toggle chkNotify;
#nullable enable
    ConstructionSiteNotifier? component;

    public void Load()
    {
        var root = fragment._root;
        chkNotify = root.AddToggle(t.T("LV.CSN.NotifyToggle"), onValueChanged: OnNotifyChanged)
            .SetMargin(top: 10);

        eb.Register(this);
    }

    void OnNotifyChanged(bool v)
    {
        if (!component) { return; }

        component!.NotifyOnCompletion = v;
    }

    [OnEvent]
    public void OnEntitySelected(SelectableObjectSelectedEvent e)
    {
        component = e.SelectableObject.GetComponent<ConstructionSiteNotifier>();
        if (!component)
        {
            component = null;
            return;
        }

        chkNotify.SetValueWithoutNotify(component.NotifyOnCompletion);
    }

    [OnEvent]
    public void OnEntityDeselected(SelectableObjectUnselectedEvent _)
    {
        component = null;
    }

}
