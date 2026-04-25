namespace RadialToolbar.UI;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class BottomBarHider(MSettings s, BottomBarPanel bottomBarPanel, EventBus eb) : ILoadableSingleton
{

    public void Load()
    {
        s.HideBottomBar.ValueChanged += (_, _) => UpdateHideBottomBar();
        eb.Register(this);
    }

    void UpdateHideBottomBar()
    {
        var hide = s.HideBottomBar.Value;
        bottomBarPanel._root.SetDisplay(!hide);
    }

    [OnEvent]
    public void OnShowPrimaryUI(ShowPrimaryUIEvent _)
    {
        UpdateHideBottomBar();
    }

}
