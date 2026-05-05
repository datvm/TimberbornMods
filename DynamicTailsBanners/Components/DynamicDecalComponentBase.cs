namespace DynamicTailsBanners.Components;

public interface IDynamicDecalComponent
{
    IDynamicDecalProvider? Provider { get; }
    bool ShowTexture();

    Material RendererMaterial { get; }
}

public abstract class DynamicDecalComponentBase<T, TProvider>(DynamicDecalService service) : BaseComponent, IDynamicDecalComponent, IAwakableComponent
    where T : DynamicDecalComponentBase<T, TProvider>
    where TProvider : IDynamicDecalProvider<T>
{
    protected readonly DynamicDecalService service = service;
    protected TProvider? provider;

    public IDynamicDecalProvider? Provider => provider;
    public abstract Material RendererMaterial { get; }

    public DynamicDecalOption Options { get; private set; } = null!;

    public virtual void Awake()
    {
        Options = GetComponent<DynamicDecalOption>();
    }

    protected abstract void ShowTexture(Texture2D texture);

    protected TProvider? GetProvider(Decal decal) => service.GetProvider<TProvider>(decal.Id);

    public void OnDecalApplied(Decal decal)
    {
        if (decal.Id == provider?.Id || !Enabled) { return; }

        if (provider is not null)
        {
            provider.Unregister((T)this);
            Options.Clear(); // Don't call this outside, because on load, it will get cleared
        }

        provider = GetProvider(decal);

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

}
