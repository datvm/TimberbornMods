namespace TImprove4Ui.Services;

public class MaterialCounterService(
    TopBarPanel topBarPanel,
    ISingletonLoader loader,
    EventBus eb
) : ILoadableSingleton, IPostLoadableSingleton, ISaveableSingleton, IUnloadableSingleton
{
    public static MaterialCounterService? Instance { get; private set; }

    static readonly SingletonKey SaveKey = new("MaterialCounter");
    static readonly ListKey<string> ProducedGoodsKey = new("ProducedGoods");

    HashSet<string> producedGoods = [];
    ImmutableArray<ExtendableCounterVisibilityManager> extendableCounters;

    public void Load()
    {
        Instance = this;
        LoadSavedData();
    }

    public void PostLoad()
    {
        var shouldClose = MSettings.AutoExpandCounter;

        List<ExtendableCounterVisibilityManager> extendableCounters = [];
        foreach (var counter in topBarPanel._counters)
        {
            if (counter is not ExtendableTopBarCounter extendableCounter) { continue; }
            
            ExtendableCounterVisibilityManager item = new(extendableCounter);
            if (shouldClose)
            {
                item.ToggleVisibility(false);
            }

            extendableCounters.Add(item);
        }

        this.extendableCounters = [.. extendableCounters];

        // Do not register this in Load or before extendableCounters is ready
        eb.Register(this);
    }

    public bool HasProducedGood(string id) => producedGoods.Contains(id);
    public void AddProducedGood(string id) => producedGoods.Add(id);

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(ProducedGoodsKey, producedGoods);
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(ProducedGoodsKey))
        {
            producedGoods = [..s.Get(ProducedGoodsKey)];
        }
    }

    public void Unload()
    {
        Instance = null;
    }

    [OnEvent]
    public void OnToolEntered(ToolEnteredEvent ev)
    {
        if (!MSettings.AutoExpandCounter || ev.Tool is CursorTool) { return; }

        foreach (var counter in extendableCounters)
        {
            counter.OpenByTool();
        }
    }

    [OnEvent]
    public void OnToolExited(ToolExitedEvent ev)
    {
        if (!MSettings.AutoExpandCounter || ev.Tool is CursorTool) { return; }

        foreach (var counter in extendableCounters)
        {
            counter.CloseByTool();
        }
    }

}
