namespace Omnibar.Services.Providers;

public class OmnibarToolItem : IOmnibarItem
{
    public string Title { get; }
    public string? Description { get; }
    public Sprite? Sprite { get; }

    public ToolButton ToolButton { get; }

    public OmnibarToolItem(ToolButton toolButton)
    {
        ToolButton = toolButton;

        var desc = toolButton.Tool.Description();
        Debug.Log(toolButton.Tool.GetType() + ": " + desc?.Title);
        Title = desc?.Title ?? "?";
        

        try
        {
            Sprite = toolButton.Root.Q("ToolImage")?.style.backgroundImage.value.sprite;
        }
        catch (Exception)
        {
            Sprite = null;
        }
    }

    public void Execute()
    {
        ToolButton.Select();
    }

    public bool SetIcon(Image image)
    {
        if (Sprite is null) { return false; }

        image.sprite = Sprite;
        return true;
    }
}
