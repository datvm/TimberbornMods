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
    Toggle chkNonblocking;
#nullable enable
    ConstructionSiteNotifier? component;

    public void Load()
    {
        var root = fragment._root;
        var panel = root.AddChild().SetMargin(top: 10);

        chkNotify = panel.AddToggle(t.T("LV.CSN.NotifyToggle"), onValueChanged: OnNotifyChanged)
            .SetMarginBottom(10);
        chkNonblocking = panel.AddToggle(t.T("LV.CSN.NonBlocking"), onValueChanged: OnNonBlockingChanged)
            .SetMargin(left: 20);

        eb.Register(this);
    }

    void OnNotifyChanged(bool v)
    {
        if (!component) { return; }

        component!.NotifyOnCompletion = v;
        SetCheckboxUI();
    }
    
    void OnNonBlockingChanged(bool v)
    {
        if (!component) { return; }
        component!.NonBlocking = v;
    }

    void SetCheckboxUI()
    {
        chkNonblocking.enabledSelf = component!.NotifyOnCompletion;
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
        chkNonblocking.SetValueWithoutNotify(component.NonBlocking);
        SetCheckboxUI();
    }

    [OnEvent]
    public void OnEntityDeselected(SelectableObjectUnselectedEvent _)
    {
        component = null;
    }

}
