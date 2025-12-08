namespace ConfigurableToolGroups.Services;

#pragma warning disable CS9113 // Parameter to ensure DI loading order
class DummyBottomBarModule(ModdableCustomToolButtonService _) : BottomBarModule([], [], []);
#pragma warning restore CS9113

public class ModdableCustomToolButtonService(
    ISpecService specs,
    IEnumerable<CustomBottomBarElement> elements,
    IEnumerable<IBottomBarElementsRemover> removes
) : ILoadableSingleton, IUnloadableSingleton
{
    public static ModdableCustomToolButtonService? Instance { get; private set; }

    public static readonly ImmutableArray<RootElementLocation> Locations = [.. Enum.GetValues(typeof(RootElementLocation))
        .Cast<RootElementLocation>()
        .OrderBy(q => q)];

    public readonly FrozenDictionary<string, CustomBottomBarElement> ElementsByIds = elements.ToFrozenDictionary(q => q.Id);

    public FrozenDictionary<RootElementLocation, ImmutableArray<CustomBottomBarElement>> CustomRootElementsByLocation { get; private set; } = FrozenDictionary<RootElementLocation, ImmutableArray<CustomBottomBarElement>>.Empty;

    public readonly FrozenSet<Type> RemovingElementTypes = ParseRemovingTypes(removes);
    static FrozenSet<Type> ParseRemovingTypes(IEnumerable<IBottomBarElementsRemover> removes)
    {
        var result = removes.SelectMany(q => q.RemovingTypes).ToFrozenSet();

        foreach (var t in result)
        {
            if (!typeof(IBottomBarElementsProvider).IsAssignableFrom(t))
            {
                throw new ArgumentException($"RemovingTypes type '{t.FullName}' does not implement {nameof(IBottomBarElementsProvider)}");
            }
        }

        return result;
    }

    public void Load()
    {
        Instance = this;

        var elSpecs = specs.GetSpecs<CustomBottomBarElementSpec>().ToDictionary(q => q.Id);

        Dictionary<RootElementLocation, List<CustomBottomBarElement>> rootByLocations = [];
        Dictionary<Type, IBottomBarElementsRemover> replacements = [];

        foreach (var location in Locations)
        {
            rootByLocations[location] = [];
        }

        foreach (var el in ElementsByIds.Values)
        {
            var id = el.Id;

            if (!elSpecs.TryGetValue(id, out var spec))
            {
                throw new ArgumentException($"No {nameof(CustomBottomBarElementSpec)} found for {nameof(CustomBottomBarElement)} with Id '{id}' (type: '{el.GetType().FullName}')");
            }
            el.CustomBottomBarElementSpec = spec;

            if (RemovingElementTypes.Contains(el.GetType())) { continue; }

            var rootSpec = spec.GetSpec<CustomRootElementSpec>();
            if (rootSpec is not null)
            {
                rootByLocations.GetOrAdd(rootSpec.Location).Add(el);
            }
        }

        CustomRootElementsByLocation = rootByLocations.ToFrozenDictionary(
            q => q.Key,
            q => q.Value.OrderBy(q => q.CustomBottomBarElementSpec.Order).ToImmutableArray()
        );
    }

    public void InitializeSection(BottomBarPanel panel, RootElementLocation location)
    {
        var container = panel._mainElements.Q(location + "Section");

        foreach (var provider in GetRootProvidersFor(panel, location))
        {
            Debug.Log(provider.GetType().FullName);

            foreach (var el in provider.GetElements())
            {
                panel.AddElement(el, container);
            }
        }
    }

    const int OriginalBaseOrder = (int)1E6;
    public IEnumerable<IBottomBarElementsProvider> GetRootProvidersFor(BottomBarPanel panel, RootElementLocation location)
    {
        // First, gather the original providers
        var providers = location switch
        {
            RootElementLocation.Left => panel._bottomBarModules
                .SelectMany(q => q.LeftElements)
                .Where(q => !RemovingElementTypes.Contains(q.Value.GetType()))
                .Select(q => new OrderedElementProvider(q.Key + OriginalBaseOrder, q.Value)),

            RootElementLocation.Middle or
            RootElementLocation.Right => panel._bottomBarModules
                .SelectMany(q => location == RootElementLocation.Middle ? q.MiddleElements : q.RightElements)
                .Where(q => !RemovingElementTypes.Contains(q.GetType()))
                .Select((q, i) => new OrderedElementProvider(i * 10 + OriginalBaseOrder, q)),

            _ => throw new ArgumentOutOfRangeException(nameof(location), location, $"Unknown location: {location}"),
        };

        // Then, insert declared elements
        return CustomRootElementsByLocation[location]
            .Select(q => new OrderedElementProvider(q.CustomBottomBarElementSpec.Order, q))
            .Concat(providers)
            .OrderBy(q => q.Order)
            .Select(q => q.Provider);
    }

    public void Unload()
    {
        Instance = null;
    }

    readonly record struct OrderedElementProvider(int Order, IBottomBarElementsProvider Provider);
}
