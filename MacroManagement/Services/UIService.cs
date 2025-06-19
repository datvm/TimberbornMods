namespace MacroManagement.Services;

public class UIService(
#pragma warning disable CS9113 // Just to make sure in run after the IEntityPanel
    IEntityPanel _,
#pragma warning restore CS9113 // Parameter is unread.
    MultiSelectFragment multiSelectFragment,
    MMFragment mmFragment
) : ILoadableSingleton
{
    public void Load()
    {
        MoveToTop(multiSelectFragment.Panel);
        MoveToTop(mmFragment.Panel);
    }

    void MoveToTop(VisualElement el)
    {
        var parent = el.parent;
        parent.Insert(0, el);
    }

}
