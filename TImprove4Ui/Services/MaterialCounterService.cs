namespace TImprove4Ui.Services;

public class MaterialCounterService(
    TopBarPanel topBarPanel,
    ISingletonLoader loader
) : ILoadableSingleton, IPostLoadableSingleton, ISaveableSingleton, IUnloadableSingleton
{
    public static MaterialCounterService? Instance { get; private set; }

    static readonly SingletonKey SaveKey = new("MaterialCounter");
    static readonly ListKey<string> ProducedGoodsKey = new("ProducedGoods");

    HashSet<string> producedGoods = [];

    const string RootName = "Counter";

    public void Load()
    {
        Instance = this;

        LoadSavedData();
    }

    public void PostLoad()
    {
        var shouldClose = MSettings.AutoExpandCounter;

        foreach (var counter in topBarPanel._counters)
        {
            if (counter is not ExtendableTopBarCounter extendableCounter) { continue; }

            var root = FindRootFor(extendableCounter);
            var items = root.Q("CounterItems");
            var toggler = root.Q<Button>("ExtensionToggler");
            var bg = root.Q("Background");

            var openByAuto = false;

            if (shouldClose)
            {
                ToggleVisibility(toggler, items, bg, false);
            }

            root.RegisterCallback<MouseEnterEvent>(_ =>
            {
                if (!MSettings.AutoExpandCounter) { return; }
                openByAuto = ToggleVisibility(toggler, items, bg, true);
            });

            root.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                if (!MSettings.AutoExpandCounter || !openByAuto) { return; }
                ToggleVisibility(toggler, items, bg, false);
                openByAuto = false;
            });
        }
    }

    public static bool ToggleVisibility(Button toggler, VisualElement items, VisualElement background, bool visible)
    {
        var isVisible = items.IsDisplayed();
        if (isVisible == visible) { return false; }

        TopBarCounterFactory.ToggleVisibility(toggler, items, background);
        return true;
    }

    public bool HasProducedGood(string id) => producedGoods.Contains(id);
    public void AddProducedGood(string id) => producedGoods.Add(id);

    static VisualElement FindRootFor(ExtendableTopBarCounter counter)
    {
        VisualElement root = counter._value;
        
        while (root.name != RootName)
        {
            root = root.parent;

            if (root is null)
            {
                throw new InvalidOperationException($"Could not find {RootName} for {counter}. The game UI was probably updated.");
            }
        }

        return root;
    }

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
}
