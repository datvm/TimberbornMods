namespace Omnibar.Services.Omnibar.Providers;

public class OmnibarBatchControlProvider(
    IEnumerable<BatchControlModule> modules,
    ILoc t,
    IAssetLoader assets,
    BatchControlBoxOpener batchControlBoxOpener
) : IOmnibarCommandProvider, ILoadableSingleton
{

    public IEnumerable<OmnibarBatchControlItem> Items { get; private set; } = null!;
    public string Command { get; } = OmnibarBatchControlItem.CommandPrefix;
    public string CommandDesc { get; } = "LV.OB.CommandDescManage".T(t);
    public Texture2D Icon { get; } = assets.Load<Texture2D>("UI/Images/Game/batch-control");

    public void Load()
    {
        Items = [.. modules.SelectMany(q => q.Tabs.Values)
            .Select(q => new OmnibarBatchControlItem(q, t, assets, batchControlBoxOpener))];
    }

    public IReadOnlyList<OmnibarFilteredItem> ProvideItems(string filter)
    {
        if (filter.IsCommand() && !filter.StartsWith(OmnibarBatchControlItem.CommandPrefix))
        {
            return [];
        }

        return OmnibarUtils.StandardFilter(Items, filter, q => q.Title);
    }


}

public class OmnibarBatchControlItem(
    BatchControlTab tab,
    ILoc t,
    IAssetLoader assets,
    BatchControlBoxOpener batchControlBoxOpener
) : IOmnibarItem
{
    public const string CommandPrefix = "/manage ";

    public string Title { get; } = CommandPrefix + t.T(tab.TabNameLocKey);
    public IOmnibarDescriptor? Description { get; } = new SimpleLabelDescriptor("LV.OB.BatchControlDesc".T(t, t.T(tab.TabNameLocKey)));
    public Sprite? Sprite { get; } = tab.TabImage is null ? null :
        assets.Load<Sprite>(Path.Combine(BatchControlBoxTabController.SpriteDirectory, tab.TabImage));

    public void Execute()
    {
        batchControlBoxOpener.OpenTab(tab);
    }

    public bool SetIcon(Image image)
    {
        if (Sprite is null) { return false; }

        image.sprite = Sprite;
        return true;
    }
}
