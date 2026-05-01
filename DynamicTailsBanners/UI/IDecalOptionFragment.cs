namespace DynamicTailsBanners.UI;

public interface IDecalOptionFragment
{
    string Id { get; }
    bool Visible { get; }

    VisualElement InitializeFragment();
    void ShowFragment(DecalSupplier decalSupplier);
    void UpdateFragment();
    void ClearFragment();
}
