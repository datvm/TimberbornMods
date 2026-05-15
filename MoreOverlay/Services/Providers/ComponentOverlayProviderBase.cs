namespace MoreOverlay.Services.Providers;

public abstract class ComponentOverlayProviderBase<TComp, TInstance> : IMoreOverlayProvider
    where TComp : BaseComponent
    where TInstance : ComponentOverlayInstanceBase<TComp>
{

    protected abstract TInstance? CreateInstance(MoreOverlayComponent overlayComp, TComp comp);

    public virtual bool TrySupporting(MoreOverlayComponent comp, [NotNullWhen(true)] out IMoreOverlayInstance? instance)
    {
        var c = comp.GetComponent<TComp>();
        instance = c ? CreateInstance(comp, c) : null;
        return instance is not null;
    }
}

public abstract class ComponentOverlayInstanceBase<T>(MoreOverlayComponent overlayComp, T comp) : IMoreOverlayInstance
    where T : BaseComponent
{
    public const int IconSize = MoreOverlayUtils.IconSize;

    protected readonly MoreOverlayComponent OverlayComponent = overlayComp;
    protected readonly T Component = comp;

    protected VisualElement el = null!;

    public virtual void Initialize(VisualElement container)
    {
        el = container.AddRow(name: GetType().Name).AlignItems().SetMarginBottom(5);
    }

    public virtual void OnHide() { }

    public virtual void OnShow() => UpdateData();

    public virtual void Remove() { }

    public abstract void UpdateData();
}
