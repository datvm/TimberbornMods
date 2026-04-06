namespace BuildingDecal.UI;

public class BuildingDecalSelectDialog : DialogBoxElement, ILoadableSingleton
{

    SpriteWithName? pickedItem;

#nullable disable
    TextField txtFilter;
#nullable enable
    readonly VisualElement groupsContainer;
    readonly List<BuildingDecalImage> images = [];

    readonly PanelStack panelStack;
    readonly BuildingDecalProvider buildingDecalProvider;

    public BuildingDecalSelectDialog(
        VisualElementInitializer veInit,
        PanelStack panelStack,
        ILoc t,
        BuildingDecalProvider buildingDecalProvider
    )
    {
        this.panelStack = panelStack;
        this.buildingDecalProvider = buildingDecalProvider;
        AddCloseButton();
        SetTitle(t.T("LV.BDl.PickDecal"));

        CreateFilterPanel(Content, t);
        CreateCommandPanel(Content, t);

        var containerScroll = Content.AddScrollView().SetHeight(400);
        groupsContainer = containerScroll.AddChild();

        this.Initialize(veInit);
    }

    VisualElement CreateFilterPanel(VisualElement parent, ILoc t)
    {
        var container = parent.AddRow().AlignItems();

        container.AddGameLabel(t.T("LV.BDl.Filter")).SetFlexGrow(0);
        txtFilter = container.AddTextField(name: "DecalFilter", changeCallback: OnFilterChanged)
            .SetFlexGrow(1);

        return container;
    }

    VisualElement CreateCommandPanel(VisualElement parent, ILoc t)
    {
        var container = parent.AddRow().AlignItems();

        container.AddMenuButton(t.T("LV.BDl.BrowseDir"), onClick: buildingDecalProvider.OpenFolder);
        container.AddMenuButton(t.T("LV.BDl.Reload"), onClick: ReloadDecals);

        return container;
    }

    public void Load()
    {
        ShowImages();
    }

    void ReloadDecals()
    {
        buildingDecalProvider.ReloadDecals();
        ShowImages();
    }

    void ShowImages()
    {
        groupsContainer.Clear();
        images.Clear();
        txtFilter.text = "";

        foreach (var grp in buildingDecalProvider.GetGroups().Groups)
        {
            var panel = groupsContainer.AddCollapsiblePanel(grp.Spec.Title.Value).SetMarginBottom();
            var panelContent = panel.Container.AddRow().SetWrap();

            foreach (var d in grp.Decals)
            {
                var s = buildingDecalProvider.GetSprite(d);
                Add(s, panelContent);
            }
        }

        void Add(SpriteWithName s, VisualElement panelContent)
        {
            var img = new BuildingDecalImage(s);
            panelContent.Add(img);
            images.Add(img);

            img.RegisterCallback<ClickEvent>(_ =>
            {
                pickedItem = s;
                OnUIConfirmed();
            });
        }
    }

    void OnFilterChanged(string text)
    {
        var lc = text.ToLower();
        foreach (var i in images)
        {
            i.Filter(lc);
        }
    }

    public async Task<SpriteWithName?> ShowPickerAsync()
    {
        var ok = await ShowAsync(null, panelStack);

        var result = ok ? pickedItem : null;
        pickedItem = null;
        return result;
    }

}
