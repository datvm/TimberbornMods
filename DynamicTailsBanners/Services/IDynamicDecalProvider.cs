namespace DynamicTailsBanners.Services;

public interface IDynamicDecalProvider
{
    string Id { get; }
}

public interface IDynamicDecalProvider<T> : IDynamicDecalProvider where T : BaseComponent
{
    void Register(T comp);
    void Unregister(T comp);

    Texture2D GetTexture(T comp);
}

public interface IDynamicTailDecalProvider : IDynamicDecalProvider<DynamicTailDecalApplier>;
