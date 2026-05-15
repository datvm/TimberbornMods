namespace MoreOverlay.Services;

public interface IMoreOverlayProvider
{

    int Order => 0;

    bool TrySupporting(MoreOverlayComponent comp, [NotNullWhen(true)] out IMoreOverlayInstance? instance);

}

public interface IMoreOverlayInstance
{
    void Initialize(VisualElement container);
    void Remove();

    void OnShow();
    void OnHide();
}

public interface ITickableMoreOverlayInstance : IMoreOverlayInstance
{
    void OnTickUpdate();
}