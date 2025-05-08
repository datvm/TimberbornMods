namespace Omnibar.UI;

public class OmnibarListItem : VisualElement
{
    public const int ItemHeight = 42;

    readonly Image icon;
    readonly Label lblTitle;

    readonly Label lblSimpleDesc;
    readonly VisualElement descriptionPanel;
    readonly Texture2D question;

    public int Index { get; private set; }
    public bool IsSelected { get; private set; }

    public OmnibarFilteredItem? FilteredItem { get; private set; }

    public event Action<OmnibarListItem> OnSelected = null!;

    public OmnibarListItem(Texture2D question)
    {
        this.question = question;

        this.SetAsRow();
        style.alignContent = Align.Center;

        icon = this.AddImage()
            .SetSize(32, 32)
            .SetMarginRight();

        var container = this.AddChild();

        lblTitle = container.AddGameLabel();
        lblSimpleDesc = container.AddGameLabel().SetDisplay(false);
        descriptionPanel = container.AddRow().SetDisplay(false);

        RegisterCallback<MouseEnterEvent>(_ => OnMouseEnter());
    }

    public void SetItem(int index, in OmnibarFilteredItem filteredItem)
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
    }

    public void UnsetItem()
    {
        Index = -1;
        FilteredItem = null;
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

    void OnMouseEnter()
    {
        OnSelected(this);
    }

}
