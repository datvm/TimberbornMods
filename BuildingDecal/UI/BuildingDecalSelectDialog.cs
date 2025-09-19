namespace BuildingDecal.UI;

public class BuildingDecalSelectDialog : DialogBoxElement, ILoadableSingleton
{

    SpriteWithName? pickedItem;

#nullable disable
    TextField txtFilter;
#nullable enable
    readonly VisualElement imageContainer;
    readonly List<BuildingDecalImage> images = [];

    readonly PanelStack panelStack;
    readonly DecalPictureService decalPictureService;

    public BuildingDecalSelectDialog(VisualElementInitializer veInit, PanelStack panelStack, ILoc t, DecalPictureService decalPictureService)
    {
        this.panelStack = panelStack;
        this.decalPictureService = decalPictureService;

        AddCloseButton();
        SetTitle(t.T("LV.BDl.PickDecal"));

        CreateFilterPanel(Content, t);
        CreateCommandPanel(Content, t);

        var containerScroll = Content.AddScrollView().SetHeight(400);
        imageContainer = containerScroll.AddRow().SetWrap();

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

        container.AddMenuButton(t.T("LV.BDl.BrowseDir"), onClick: decalPictureService.OpenFolder);
        container.AddMenuButton(t.T("LV.BDl.Reload"), onClick: ReloadDecals);

        return container;
    }

    public void Load()
    {
        ShowImages();
    }

    void ReloadDecals()
    {
        decalPictureService.ReloadDecals();
        ShowImages();
    }

    void ShowImages()
    {
        imageContainer.Clear();
        images.Clear();
        txtFilter.text = "";

        Add(decalPictureService.ErrorIcon);
        foreach (var s in decalPictureService.DecalsList)
        {
            Add(s);
        }

        void Add(SpriteWithName s)
        {
            var img = new BuildingDecalImage(s);
            imageContainer.Add(img);
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
