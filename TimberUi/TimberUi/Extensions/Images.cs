namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

    public static Image AddImage(this VisualElement parent, string? name = default, IEnumerable<string>? additionalClasses = default)
    {
        return parent.AddChild<Image>(name, classes: additionalClasses);
    }

    public static Image AddImage(this VisualElement parent, string className, string? name = default, IEnumerable<string>? additionalClasses = default)
    {
        return parent.AddImage(name: name, additionalClasses: [className, .. additionalClasses ?? []]);
    }

    public static Image AddImage<T>(this VisualElement parent, T texture, string? name = default, IEnumerable<string>? additionalClasses = default) where T : Texture
    {
        var i = parent.AddImage(name: name, additionalClasses: additionalClasses);
        i.image = texture;
        return i;
    }

    public static Image AddImage<TTexture>(this VisualElement parent, string path, IAssetLoader assetLoader, string? name = default, IEnumerable<string>? additionalClasses = default) where TTexture : Texture
    {
        var texture = assetLoader.Load<TTexture>(path);
        return parent.AddImage(texture, name, additionalClasses);
    }

    public static Image AddImage(this VisualElement parent, string path, IAssetLoader assetLoader, string? name = default, IEnumerable<string>? additionalClasses = default)
        => parent.AddImage<Texture>(path, assetLoader, name, additionalClasses);

    public static readonly ImmutableArray<string> InventoryOutputClasses = ["inventory-row-informational__type", "inventory-row-informational__type--output"];
    public static readonly ImmutableArray<string> InventoryInputClasses = ["inventory-row-informational__type", "inventory-row-informational__type--input"];
    public static Image AddInventoryIoImage(this VisualElement parent, bool isOutput)
    {
        return AddImage(parent, additionalClasses: isOutput ? InventoryOutputClasses : InventoryInputClasses);
    }

    public static Image AddInventoryInputImage(this VisualElement parent) => AddInventoryIoImage(parent, false);
    public static Image AddInventoryOutputImage(this VisualElement parent) => AddInventoryIoImage(parent, true);


}