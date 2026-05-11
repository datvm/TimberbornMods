namespace DynamicTailsBanners.Components;

public interface IDynamicDecalComponent
{
    IDynamicDecalProvider? Provider { get; }
    bool ShowTexture();

    Material RendererMaterial { get; }
}

public abstract class DynamicDecalComponentBase<T, TProvider>(DynamicDecalService service) : BaseComponent, IDynamicDecalComponent, IAwakableComponent, IDeletableEntity
    where T : DynamicDecalComponentBase<T, TProvider>
    where TProvider : IDynamicDecalProvider<T>
{
    protected readonly DynamicDecalService service = service;
    protected TProvider? provider;

    public IDynamicDecalProvider? Provider => provider;
    public abstract Material RendererMaterial { get; }

    public DynamicDecalOption Options { get; private set; } = null!;
    Decal? currDecal;

    public virtual void Awake()
    {
        Options = GetComponent<DynamicDecalOption>();
    }

    protected abstract void ShowTexture(Texture2D texture);

    protected TProvider? GetProvider(Decal decal) => service.GetProvider<TProvider>(decal.Id);

    public void OnDecalApplied(Decal decal)
    {
        if (decal.Id == provider?.Id || !Enabled) { return; }

        currDecal = decal;
        ReregisterProvider(false);
    }

    public void UnregisterProvider(bool keepOptions)
    {
        if (provider is not null)
        {
            provider.Unregister((T)this);
            provider = default;
            if (!keepOptions)
            {
                Options.Clear();
            }
        }
    }

    public void ReregisterProvider(bool keepOptions)
    {
        UnregisterProvider(keepOptions);

        if (currDecal is null) { return; }

        provider = GetProvider(currDecal.Value);

        if (provider is IConnectedDynamicDecal c)
        {
            Options.SetSize(c.ExpectedConnectionCount);
        }

        provider?.Register((T)this);
    }

    public bool ShowTexture()
    {
        if (provider is null || !Enabled) { return false; }

        var texture = provider.GetTexture((T)this);
        ShowTexture(texture);
        return true;
    }

    public void DeleteEntity()
    {
        UnregisterProvider(false);
    }
}
