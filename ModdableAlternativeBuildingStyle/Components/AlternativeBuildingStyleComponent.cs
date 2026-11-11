namespace ModdableAlternativeBuildingStyle.Components;

[AddTemplateModule2(typeof(AlternativeBuildingStyleSpec))]
public class AlternativeBuildingStyleComponent(AlternativeBuildingStyleService service) : BaseComponent, IAwakableComponent, IPersistentEntity, IInitializableEntity, IDeletableEntity, IDuplicable<AlternativeBuildingStyleComponent>, IPostPlacementChangeListener
{
    static readonly ComponentKey SaveKey = new(nameof(AlternativeBuildingStyleComponent));
    static readonly PropertyKey<string> AlternativeStyleKey = new("AlternativeStyle");

    public event EventHandler<string?>? AlternativeStyleChanged;

    bool ready;
    bool hasPending;
    string? pendingAlternativeStyle;

    internal BuildingStyleState State { get; } = new();
    readonly List<IAlternativeBuildingStyleListener> listeners = [];

    public AlternativeBuildingStyleSpec Spec { get; private set; } = null!;
    public bool HasAlternativeStyles { get; private set; }

    public string? AlternativeStyle { get; private set; }
    public bool IsOriginal => AlternativeStyle is null;

    public IReadOnlyList<string> AlternativeStyles { get; private set; } = [];

    public void Awake()
    {
        Spec = GetComponent<AlternativeBuildingStyleSpec>();
        if (Spec is not null)
        {
            HasAlternativeStyles = Spec.Styles.Length > 0;
            if (HasAlternativeStyles)
            {
                AlternativeStyles = [.. Spec.Styles.Select(s => s.Name).OrderBy(s => s)];
            }
            else
            {
                return;
            }
        }

        GetComponents(listeners);

        service.Capture(this);
    }

    public void DuplicateFrom(AlternativeBuildingStyleComponent source)
    {
        TrySetStyle(source.AlternativeStyle);
    }

    public void TrySetStyle(string? styleName)
    {
        if (styleName is not null && !HasStyle(styleName))
        {
            return;
        }

        if (!ready)
        {
            hasPending = true;
            pendingAlternativeStyle = styleName;
            return;
        }

        if (styleName == AlternativeStyle)
        {
            return;
        }

        if (styleName is null)
        {
            service.Restore(this);
            AlternativeStyle = null;
            NotifyStyleChanged();
            return;
        }

        var style = Spec.Styles.First(s => s.Name == styleName);
        if (service.TryApply(this, style))
        {
            AlternativeStyle = styleName;
            NotifyStyleChanged();
        }
    }

    void NotifyStyleChanged()
    {
        var style = AlternativeStyle;
        foreach (var listener in listeners)
        {
            listener.OnAlternativeStyleChanged(this, style);
        }

        AlternativeStyleChanged?.Invoke(this, style);        
    }

    public void RestoreOriginalStyle()
    {
        TrySetStyle(null);
    }

    public void InitializeEntity()
    {
        ready = true;
        if (!hasPending)
        {
            return;
        }

        hasPending = false;
        var style = pendingAlternativeStyle;
        pendingAlternativeStyle = null;
        TrySetStyle(style);
    }

    public void DeleteEntity()
    {
        service.Release(this);
    }

    public void OnPostPlacementChanged()
    {
        service.SyncMovedStyle(this);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s))
        {
            return;
        }

        hasPending = true;
        pendingAlternativeStyle = s.Get(AlternativeStyleKey);
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (AlternativeStyle is not null)
        {
            var s = entitySaver.GetComponent(SaveKey);
            s.Set(AlternativeStyleKey, AlternativeStyle);
        }
    }

    bool HasStyle(string styleName)
    {
        if (!HasAlternativeStyles)
        {
            return false;
        }

        foreach (var style in Spec.Styles)
        {
            if (style.Name == styleName)
            {
                return true;
            }
        }

        return false;
    }

}
