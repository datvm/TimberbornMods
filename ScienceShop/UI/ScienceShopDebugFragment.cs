namespace ScienceShop.UI;

[BindSingleton]
public class ScienceShopDebugFragment(DebugFragmentFactory fragments) : IEntityPanelFragment
{
    VisualElement root = null!;
    ConsumeScienceManufactory? shop;

    public VisualElement InitializeFragment()
    {
        root = fragments.Create(
            "Science Shop",
            new DebugFragmentButton(FinishCurrent, "Finish current recipe"));
        return root;
    }

    public void ShowFragment(BaseComponent entity)
    {
        shop = entity.GetComponent<ConsumeScienceManufactory>();
        UpdateFragment();
    }

    public void ClearFragment()
    {
        shop = null;
        UpdateFragment();
    }

    public void UpdateFragment()
        => root.ToggleDisplayStyle(shop);

    void FinishCurrent()
    {
        if (!shop)
        {
            return;
        }

        shop!.DebugFinishCurrent();
    }
}
