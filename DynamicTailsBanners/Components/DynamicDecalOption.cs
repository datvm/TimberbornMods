namespace DynamicTailsBanners.Components;

[AddTemplateModule2(typeof(IDynamicDecalComponent))]
public class DynamicDecalOption(EntityRegistry registry) : BaseComponent, IDuplicable<DynamicDecalOption>, IPersistentEntity, IAwakableComponent
{
    static readonly ComponentKey SaveKey = new(nameof(DynamicDecalOption));
    static readonly ListKey<Guid> ComponentsKey = new("ComponentIds");
    static readonly PropertyKey<string> SettingsKey = new("Settings");

    public EntityComponent?[] Components { get; private set; } = [];
    public HashSet<Delegate> Handlers { get; } = [];

    string? json;
    object? settings;
    object? reference;

    public DynamicBuildingDecal? BuildingDecal { get; private set; }
    public DynamicTailDecal? TailDecal { get; private set; }

    public void Awake()
    {
        BuildingDecal = GetComponent<DynamicBuildingDecal>();
        TailDecal = GetComponent<DynamicTailDecal>();
    }

    public T? GetReference<T>() where T : class => reference as T;
    public void SetReference<T>(T reference) => this.reference = reference;
    public void SetReference(object? reference) => this.reference = reference;
    public void ClearReference() => reference = null;

    public T? GetSettings<T>() where T : class
    {
        if (settings is null && json is not null)
        {
            try
            {
                settings = JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                settings = null;
                Debug.LogWarning($"Failed to deserialize settings {typeof(T).Name} for DynamicDecalOption: {ex}");
            }

            json = null;
        }

        return settings as T;
    }

    public T GetSettingsOrThrow<T>() where T : class 
        => GetSettings<T>()
        ?? throw new InvalidOperationException($"DynamicDecalOption does not have settings of type {typeof(T).Name}");

    public T GetSettingsOrDefault<T>() where T : class, new()
    {
        var s = GetSettings<T>();
        if (s is null)
        {
            s = new T();
            SetSettings(s);
        }

        return s;
    }

    public IEnumerable<T> PopHandlers<T>() where T : Delegate
    {
        foreach (var h in Handlers.OfType<T>().ToArray())
        {
            yield return h;
            Handlers.Remove(h);
        }
    }

    public void SetSettings<T>(T settings) where T : class
    {
        this.settings = settings;
    }

    public void SetDefault<T>() where T : class, new() => SetDefault(() => new T());

    public void SetDefault<T>(Func<T> settings) where T : class
    {
        if (GetSettings<T>() is null)
        {
            SetSettings(settings());
        }
    }

    public void Clear()
    {
        settings = reference = json = null;

        if (Handlers.Count > 0)
        {
            var provider = GetComponent<IDynamicDecalComponent>()
                ?? throw new InvalidOperationException("The DynamicDecalOption has registered handlers but no provider component. This should not happen.");
            throw new InvalidOperationException($"{provider} left registered handlers. It should have cleared them in the Unregister calls");
        }
    }

    public void SetSize(int size)
    {
        if (Components.Length == size)
        {
            return;
        }
        else if (Components.Length > size)
        {
            Components = [.. Components.Take(size)];
        }
        else
        {
            var newComponents = new EntityComponent[size];
            Array.Copy(Components, newComponents, Components.Length);

            Components = newComponents;
        }
    }

    public async void DuplicateFrom(DynamicDecalOption source)
    {
        await Awaitable.NextFrameAsync(); // Let the Decal ID copied first

        var min = Math.Min(Components.Length, source.Components.Length);

        for (int i = 0; i < min; i++)
        {
            Components[i] = source.Components[i];
        }

        // Deep copy the settings, do NOT just copy the reference
        if (settings is not null)
        {
            var json = JsonConvert.SerializeObject(source.settings);
            settings = JsonConvert.DeserializeObject(json, settings.GetType());
        }

        // Do NOT copy reference

        var buildingDecal = GetComponent<DynamicBuildingDecal>();
        if (buildingDecal)
        {
            buildingDecal.ReregisterProvider(true);
        }
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        var ids = s.Get(ComponentsKey);
        Components = [.. ids
            .Select(id => id == Guid.Empty ? null : registry.GetEntity(id))];

        if (s.Has(SettingsKey))
        {
            json = s.Get(SettingsKey);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(ComponentsKey, [.. Components.Select(c => c?.EntityId ?? Guid.Empty)]);

        if (settings is not null)
        {
            var json = JsonConvert.SerializeObject(settings);
            s.Set(SettingsKey, json);
        }
    }

    
}
