namespace TImprove4Ui.Services;

[BindSingleton]
public class BatchControlBoxService(
    IBatchControlBox iBatchControlBox,
    ISingletonLoader loader,
    MSettings s
) : ILoadableSingleton, ISaveableSingleton, IUnloadableSingleton
{
    public static BatchControlBoxService? Instance;

    static readonly SingletonKey SaveKey = new(nameof(TImprove4Ui));
    static readonly PropertyKey<bool> ReverseBatchControlBoxKey = new("BatchControlBoxReverse");

    readonly BatchControlBox batchControlBox = (BatchControlBox)iBatchControlBox;
    VisualElement batchControlBoxRoot = null!;
    NineSliceButton? btnSwap;

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

    void RefreshVisibility() => btnSwap?.SetDisplay(s.ShowSwapManagementLocationButton.Value);

    void AddSwapLocationButton()
    {
        var tabButtons = batchControlBoxRoot.Q("TabButtons");

        var container = batchControlBoxRoot.AddChild();
        container.style.justifyContent = Justify.Center;
        container.InsertSelfAfter(tabButtons);

        btnSwap = container.AddMenuButton("↔", onClick: SwapPanelLocation)            
            .SetSize(48)
            .SetFlexGrow(0)
            .SetFlexShrink(0);
        btnSwap.style.minWidth = 48;

        RefreshVisibility();
        s.ModSettingChanged += (_, _) => RefreshVisibility();
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
