namespace MoreOverlay.Services;

[BindSingleton]
public class MoreOverlayService(
    IEnumerable<IMoreOverlayProvider> providers, 
    VisualElementLoader veLoader,
    EntitySelectionService entitySelectionService,
    StockpileOverlay stockpileOverlay,
    MoreOverlayHighlighter highlighter
) : ILoadableSingleton, IUnloadableSingleton
{
    public static MoreOverlayService? Instance { get; private set; }

    public readonly ImmutableArray<IMoreOverlayProvider> Providers = [.. providers.OrderBy(p => p.Order)];

    public bool IsOverlayActive => stockpileOverlay._enabled;
    public event EventHandler<bool> OnOverlayToggled = null!;

    public void Load()
    {
        Instance = this;
        OnOverlayToggled += InternalOnOverlayToggled;
    }

    void InternalOnOverlayToggled(object sender, bool e)
    {
        if (!e)
        {
            highlighter.UnhighlightAll();
        }
    }

    public IEnumerable<IMoreOverlayInstance> GetProviders(MoreOverlayComponent comp)
    {
        if (comp.HasComponent<StockpileOverlayItemAdder>()) { yield break; }

        foreach (var p in Providers)
        {
            if (p.TrySupporting(comp, out var instance))
            {
                yield return instance;
            }
        }
    }

    public (VisualElement panel, VisualElement container) CreatePanel(MoreOverlayComponent comp)
    {
        var e = veLoader.LoadVisualElement("Game/StockpileOverlayItem");
        
        var btn = e.Q<Button>("EntityButton");
        var s = btn.style;
        s.height = new StyleLength(StyleKeyword.Auto);
        s.flexDirection = FlexDirection.Column;
        s.alignItems = Align.FlexStart;
        s.justifyContent = Justify.Center;
        s.paddingTop = 5;

        btn.Clear();
        btn.AddAction(() => entitySelectionService.Select(comp));
        
        e.Q("SelectionButton")?.RemoveFromHierarchy();

        return (e, btn);
    }

    public void AddOverlay(VisualElement panel, Vector3 worldPos) => stockpileOverlay.Add(panel, worldPos);

    public void RemoveOverlay(VisualElement panel) => stockpileOverlay.Remove(panel);

    internal void SetOverlayActive(bool active)
    {
        OnOverlayToggled?.Invoke(this, active);
    }

    public void Unload()
    {
        Instance = null;
    }
}
