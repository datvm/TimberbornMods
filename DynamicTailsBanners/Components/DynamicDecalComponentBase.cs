namespace DynamicTailsBanners.Components;

public interface IDynamicDecalComponent
{
    bool ShowTexture();
}

public abstract class DynamicDecalComponentBase<T, TProvider>(DynamicDecalService service) : BaseComponent, IDynamicDecalComponent
    where T : DynamicDecalComponentBase<T, TProvider>
    where TProvider : IDynamicDecalProvider<T>
{
    protected readonly DynamicDecalService service = service;
    protected TProvider? provider;

    protected abstract void ShowTexture(Texture2D texture);

    protected TProvider? GetProvider(Decal decal) => service.GetProvider<TProvider>(decal.Id);

    public void OnDecalApplied(Decal decal)
    {
        if (decal.Id == provider?.Id || !Enabled) { return; }

        provider?.Unregister((T)this);

        provider = GetProvider(decal);
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
