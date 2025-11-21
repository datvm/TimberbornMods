namespace ConfigurableTopBar.UI;

public class SelectIconDialog : DialogBoxElement
{
    readonly record struct IconEntry(AssetRef<Sprite> Sprite, Image Icon);

    readonly List<IconEntry> entries = [];
    readonly PanelStack panelStack;

    AssetRef<Sprite>? selectedIcon;

    public SelectIconDialog(GoodSpriteProvider goodSpriteProvider, ILoc t, PanelStack panelStack, VisualElementInitializer veInit)
    {
        this.panelStack = panelStack;

        AddCloseButton();

        var filterPanel = Content.AddChild().SetMarginBottom();
        filterPanel.AddLabel(t.T("LV.CTB.Filter"));
        filterPanel.AddTextField(changeCallback: OnFilterChanged)
            .SetMarginBottom();

        AddIcons(t.T("LV.CTB.GoodGroups"), goodSpriteProvider.GoodGroups.Values);
        AddIcons(t.T("LV.CTB.Goods"), goodSpriteProvider.Goods.Values);

        this.Initialize(veInit);
    }

    void OnFilterChanged(string filter)
    {
        filter = filter.Trim();
        var noKw = filter.Length == 0;

        foreach (var e in entries)
        {
            e.Icon.SetDisplay(noKw || e.Sprite.Path.Contains(filter, StringComparison.InvariantCultureIgnoreCase));
        }
    }

    void AddIcons(string header, IEnumerable<AssetRef<Sprite>> icons)
    {
        var panel = Content.AddChild().SetMarginBottom();

        panel.AddLabelHeader(header);
        var grid = panel.AddRow();
        grid.style.flexWrap = Wrap.Wrap;
        grid.style.justifyContent = Justify.Center;

        foreach (var icon in icons)
        {
            var img = grid.AddImage().SetSize(50, 50).SetMargin(10);
            img.sprite = icon.Asset;
            img.RegisterCallback<ClickEvent>(e =>
            {
                selectedIcon = icon;
                OnUIConfirmed();
            });

            entries.Add(new(icon, img));
        }
    }

    public async Task<AssetRef<Sprite>?> ShowAsync()
        => await ShowAsync(null, panelStack) ? selectedIcon : null;

}
