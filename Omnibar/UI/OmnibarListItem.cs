namespace Omnibar.UI;

public class OmnibarListItem : NineSliceVisualElement
{
    public static readonly Color SelectingColor = new(.835f, .831f, .721f, 1f);

    readonly Image icon;
    readonly Label lblTitle;

    readonly Label lblSimpleDesc;
    readonly VisualElement descriptionPanel;
    readonly Texture2D question;

    public int Index { get; private set; }
    public bool IsSelected { get; private set; }

    public OmnibarFilteredItem? FilteredItem { get; private set; }

    public OmnibarListItem(Texture2D question)
    {
        this.question = question;

        this.SetAsRow()
            .SetPadding(5)
            .AddClasses("clickable-hierarchy");
        style.alignItems = Align.Center;
        style.borderLeftWidth = style.borderRightWidth =
            style.borderTopWidth = style.borderBottomWidth = 2;

        icon = this.AddImage()
            .SetSize(32, 32)
            .SetMarginRight();

        var container = this.AddChild();

        lblTitle = container.AddGameLabel();
        lblSimpleDesc = container.AddGameLabel().SetDisplay(false);
        descriptionPanel = container.AddRow().SetDisplay(false);
    }

    public void SetItem(int index, in OmnibarFilteredItem filteredItem, int selectingIndex)
    {
        Index = index;
        FilteredItem = filteredItem;
        var item = filteredItem.Item;

        SetItemTitle();
        if (item.Description is not null)
        {
            lblSimpleDesc.text = item.Description;
            lblSimpleDesc.SetDisplay(true);
            descriptionPanel.SetDisplay(false);
        }

        if (!item.SetIcon(icon))
        {
            icon.image = question;
        }

        SetSelectedIndex(selectingIndex);
    }

    public void UnsetItem()
    {
        Index = -1;
        FilteredItem = null;
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
        StringBuilder title = new(FilteredItem!.Value.Item.Title);
        var chars = FilteredItem.Value.Match.Positions;

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
