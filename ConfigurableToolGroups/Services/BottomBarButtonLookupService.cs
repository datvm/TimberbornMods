namespace ConfigurableToolGroups.Services;

public class BottomBarButtonLookupService : IUnloadableSingleton
{
    public static BottomBarButtonLookupService? Instance { get; private set; }

    readonly Dictionary<VisualElement, ToolButton> toolButtonsMapping = [];
    readonly Dictionary<VisualElement, ToolGroupButton> toolGroupButtonsMapping = []; 
    readonly Dictionary<VisualElement, BottomBarButtonLookup> elements = [];
    readonly Dictionary<string, BottomBarButtonLookup> idMapping = [];
    readonly ILoc t;
    readonly ToolUnlockingService toolUnlockingService;

    public BottomBarButtonLookupService(
        ILoc t,
        ToolUnlockingService toolUnlockingService
    )
    {
        this.t = t;
        this.toolUnlockingService = toolUnlockingService;

        Instance = this;
    }

    public void RegisterToolButton(ToolButton btn) => toolButtonsMapping[btn.Root] = btn;
    public void RegisterToolGroupButton(ToolGroupButton btn) => toolGroupButtonsMapping[btn.Root] = btn;

    public void Register(VisualElement ve, BottomBarButtonLookup info)
    {
        elements[ve] = info;

        var id = info.Id;
        if (idMapping.ContainsKey(id))
        {
            Debug.LogWarning($"Duplicate BottomBarButtonLookup id: {id}");
        }

        idMapping[id] = info;
    }

    public void Register(ToolGroupButton btn)
    {
        var el = btn.Root;
        var spec = btn._toolGroup;

        var id = $"ToolGroupButton.{spec.Id}";

        Register(el, new BottomBarButtonLookup<ToolGroupButton>(id, el, spec.DisplayNameLocKey, t.T(spec.DisplayNameLocKey), btn)
        {
            Sprite = spec.Icon.Asset,
            Activate = btn.Select,
        });
    }

    public void Register(ModdableToolGroupButton grp) => Register(grp.ToolGroupButton);

    public void Register(ToolButton btn, string id, string titleLoc)
    {
        var sprite = btn.Root.Q("ToolImage").style.backgroundImage.value.sprite;
        var tool = btn.Tool;

        var el = btn.Root;

        Register(el, new BottomBarButtonLookup<ToolButton>(id, el, titleLoc, t.T(titleLoc), btn)
        {
            Sprite = sprite,
            IsLockedFunc = () => toolUnlockingService.IsLocked(tool),
            Activate = btn.Select,
        });
    }

    public bool TryGetToolButton(VisualElement ve, out ToolButton btn) => toolButtonsMapping.TryGetValue(ve, out btn);
    public bool TryGetToolGroupButton(VisualElement ve, out ToolGroupButton btn) => toolGroupButtonsMapping.TryGetValue(ve, out btn);

    public bool TryGetById(string id, out BottomBarButtonLookup? info) => idMapping.TryGetValue(id, out info);

    public bool TryGet(VisualElement ve, out BottomBarButtonLookup? info) => TryGet<BottomBarButtonLookup>(ve, out info);
    public bool TryGet<T>(VisualElement ve, out T? info) where T : BottomBarButtonLookup
    {
        if (elements.TryGetValue(ve, out var i) && i is T t)
        {
            info = t;
            return true;
        }

        info = default;
        return false;
    }

    public void Unload()
    {
        Instance = null;
    }
}
