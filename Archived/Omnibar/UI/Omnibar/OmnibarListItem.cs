namespace Omnibar.UI;

public class OmnibarListItem : NineSliceVisualElement
{
    public static readonly Color SelectingColor = new(.835f, .831f, .721f, 1f);

    readonly Image icon;
    readonly Label lblTitle;

    readonly VisualElement descriptionPanel;
    readonly VisualElement hotkeysPanel;
    readonly Texture2D question;

    public int Index { get; private set; }
    public bool IsSelected { get; private set; }

    public OmnibarBoxItem? OmnibarItem { get; private set; }

    public OmnibarListItem(Texture2D question)
    {
        this.question = question;

        this.SetAsRow()
            .SetPadding(5)
            .AddClasses("clickable-hierarchy")
            .AlignItems();
        style.borderLeftWidth = style.borderRightWidth =
            style.borderTopWidth = style.borderBottomWidth = 2;

        icon = this.AddImage()
            .SetSize(55)
            .SetMarginRight();
        icon.image = question;

        var container = this.AddChild();

        lblTitle = container.AddGameLabel();
        descriptionPanel = container.AddRow().AlignItems().SetDisplay(false);
        hotkeysPanel = container.AddRow().AlignItems().SetDisplay(false).SetMargin(top: 5);
    }

    public void SetItem(int index, in OmnibarBoxItem omnibarItem, int selectingIndex)
    {
        Index = index;
        OmnibarItem = omnibarItem;
        var item = omnibarItem.Item;

        SetItemTitle();

        descriptionPanel.Clear();
        descriptionPanel.SetDisplay(item.Description?.Describe(descriptionPanel) == true);

        if (!item.SetIcon(icon))
        {
            icon.image = question;
        }

        if (omnibarItem.HotkeyActions.Count > 0)
        {
            SetHotkeys(omnibarItem.HotkeyActions);
        }

        SetSelectedIndex(selectingIndex);
    }

    void SetHotkeys(IReadOnlyList<IOmnibarHotkeyAction> hotkeyActions)
    {
        foreach (var hotkey in hotkeyActions.SelectMany(q => q.HotkeyPrompts))
        {
            hotkeysPanel.AddGameLabel(hotkey).SetMarginRight();
        }
        hotkeysPanel.SetDisplay(true);
    }

    public void UnsetItem()
    {
        Index = -1;
        
        descriptionPanel.Clear();
        descriptionPanel.SetDisplay(false);

        hotkeysPanel.Clear();
        hotkeysPanel.SetDisplay(false);

        icon.image = question;
        OmnibarItem = null;
        IsSelected = false;
        SetSelectionUi(false);
    }

    public void SetSelectedIndex(int index)
    {
        var shoudSelect = index == Index;
        if (shoudSelect == IsSelected) { return; }

        IsSelected = shoudSelect;
        SetSelectionUi(shoudSelect);
    }

    void SetSelectionUi(bool selected)
    {
        style.borderLeftColor = style.borderRightColor = style.borderTopColor = style.borderBottomColor =
            selected ? SelectingColor : Color.clear;
    }

    void SetItemTitle()
    {
        StringBuilder title = new(OmnibarItem!.Value.Item.Title);
        var chars = OmnibarItem.Value.Match.Positions;

        for (int i = chars.Length - 1; i >= 0; i--)
        {
            var pos = chars[i];
            var c = title[pos];
            title.Remove(pos, 1);

            title.Insert(pos, c.ToString().Bold());
        }

        lblTitle.text = title.ToString();
    }

}
