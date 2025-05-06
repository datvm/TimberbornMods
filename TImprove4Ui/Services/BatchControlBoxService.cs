namespace TImprove4Ui.Services;

public class BatchControlBoxService(
    IBatchControlBox iBatchControlBox,
    ISingletonLoader loader
) : ILoadableSingleton, ISaveableSingleton, IUnloadableSingleton
{
    public static BatchControlBoxService? Instance;

    static readonly SingletonKey SaveKey = new(nameof(TImprove4Ui));
    static readonly PropertyKey<bool> ReverseBatchControlBoxKey = new("BatchControlBoxReverse");

    readonly BatchControlBox batchControlBox = (BatchControlBox)iBatchControlBox;
    VisualElement batchControlBoxRoot = null!;

    bool batchControlBoxReverse;

    public void Load()
    {
        LoadSavedData();

        batchControlBoxRoot = batchControlBox._root;
        AddSwapLocationButton();

        Instance = this;
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }
        batchControlBoxReverse = s.Has(ReverseBatchControlBoxKey) && s.Get(ReverseBatchControlBoxKey);
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);

        if (batchControlBoxReverse)
        {
            s.Set(ReverseBatchControlBoxKey, batchControlBoxReverse);
        }
    }

    void AddSwapLocationButton()
    {
        var tabButtons = batchControlBoxRoot.Q("TabButtons");

        var container = batchControlBoxRoot.AddChild();
        container.style.justifyContent = Justify.Center;
        container.InsertSelfAfter(tabButtons);

        var btnSwap = container.AddMenuButton("↔", onClick: SwapPanelLocation)            
            .SetSize(48)
            .SetFlexGrow(0)
            .SetFlexShrink(0);
        btnSwap.style.minWidth = 48;
    }

    void SwapPanelLocation()
    {
        batchControlBoxReverse = !batchControlBoxReverse;
        UpdatePanelLocation();
    }

    public void UpdatePanelLocation()
    {
        batchControlBoxRoot.style.alignSelf = batchControlBoxReverse
            ? Align.FlexEnd
            : Align.FlexStart;
    }

    public void Unload()
    {
        Instance = null;
    }
}
