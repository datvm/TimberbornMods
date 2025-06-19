
namespace MacroManagement.UI;

public class MMFragment(
    IAssetLoader assets,
    IGoodService goodService
) : IEntityPanelFragment
{

#nullable disable
    public EntityPanelFragmentElement Panel { get; private set; }
    VisualElement itemsContainer;
#nullable enable

    MMComponent? comp;
    MMBuildingItemElement[] items = [];

    MMBuildingItemElementInjection injection;

    public void ClearFragment()
    {
        items = [];
        itemsContainer.Clear();
        comp = null;
        Panel.Visible = false;
    }

    public VisualElement InitializeFragment()
    {
        InitializeElementInjection();

        Panel = new()
        {
            Visible = false,
            Background = EntityPanelFragmentBackground.PalePurple,
        };

        itemsContainer = Panel.AddScrollView().SetMaxHeight(200);

        return Panel;
    }

    void InitializeElementInjection()
    {
        var questionMark = assets.Load<Sprite>("UI/Images/Game/question-mark");

        injection = new(goodService, questionMark);
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponentFast<MMComponent>();
        if (!comp) { return; }

        PopulateData();
        Panel.Visible = true;
    }

    void PopulateData()
    {
        var buildings = comp!.Buildings;
        var len = buildings.Length;
        items = new MMBuildingItemElement[len];

        for (int i = 0; i < len; i++)
        {
            items[i] = itemsContainer
                .AddChild<MMBuildingItemElement>()
                .SetItem(buildings[i], injection);
        }
    }

    public void UpdateFragment()
    {
        if (items.Length <= 0) { return; }

        for (int i = 0; i < items.Length; i++)
        {
            items[i].UpdateData();
        }
    }
}
