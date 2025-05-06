namespace Omnibar.UI;

public class OmnibarListItem : VisualElement
{

    readonly Image icon;
    readonly Label lblTitle;

    readonly Label lblSimpleDesc;
    readonly VisualElement descriptionPanel;
    readonly Texture2D question;

    public IOmnibarItem? Item { get; private set; }

    public OmnibarListItem(Texture2D question)
    {
        this.question = question;

        this.SetAsRow();

        icon = this.AddImage()
            .SetSize(32, 32)
            .SetMarginRight();

        var container = this.AddChild();

        lblTitle = container.AddGameLabel();
        lblSimpleDesc = container.AddGameLabel().SetDisplay(false);
        descriptionPanel = container.AddRow().SetDisplay(false);
    }

    public void SetItem(IOmnibarItem item)
    {
        Item = item;

        lblTitle.text = item.Title;
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

}
