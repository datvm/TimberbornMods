namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

    public static readonly ImmutableArray<string> InventoryOutputClasses = ["inventory-row-informational__type", "inventory-row-informational__type--output"];
    public static readonly ImmutableArray<string> InventoryInputClasses = ["inventory-row-informational__type", "inventory-row-informational__type--input"];

    extension<T>(T icon) where T : IconSpan
    {
        
        public T SetGood(IGoodService goods, string id, string? amount = null, bool showName = false, int size = 24)
        {
            var g = goods.GetGood(id);
            icon.SetContent(g.IconSmall.Value, 
                prefixText: amount,
                postfixText: showName ? g.DisplayName.Value : null,
                size: size);

            return icon;
        }
        
        public T SetScience(NamedIconProvider icons, string? amount = null, int size = 24)
        {
            icon.SetVertical()
                .SetContent(icons.Science, prefixText: amount, size: size);
            return icon;
        }

        public T SetTime(NamedIconProvider icons, string time, int size = 21)
        {
            icon.SetVertical()
                .SetContent(icons.Clock, postfixText: time, size: size);
            return icon;
        }

    }

    extension(VisualElement parent)
    {

        public IconSpan AddIconSpan() => parent.AddChild<IconSpan>();

        public IconSpan AddIconSpan(ImageSource src, string? prefixText = null, string? postfixText = null, int? size = null) 
            => parent.AddIconSpan().SetContent(src, prefixText, postfixText, size);

        public IconSpan AddIconSpan(ImageSource src, string? prefixText = null, string? postfixText = null, Vector2? size = null)
            => parent.AddIconSpan().SetContent(src, prefixText, postfixText, size);

        public IconSpan AddArrow(NamedIconProvider provider) => parent.AddIconSpan(provider.Arrow, size: 24);

        public Image AddImage(string? name = default, IEnumerable<string>? additionalClasses = default)
        {
            return parent.AddChild<Image>(name, classes: additionalClasses);
        }

        public Image AddImage(string className, string? name = default, IEnumerable<string>? additionalClasses = default)
        {
            return parent.AddImage(name: name, additionalClasses: [className, .. additionalClasses ?? []]);
        }

        public Image AddImage(Sprite sprite, string? name = default, IEnumerable<string>? additionalClasses = default)
        {
            var i = parent.AddImage(name: name, additionalClasses: additionalClasses);
            i.sprite = sprite;
            return i;
        }

        public Image AddImage<T>(T texture, string? name = default, IEnumerable<string>? additionalClasses = default) where T : Texture
        {
            var i = parent.AddImage(name: name, additionalClasses: additionalClasses);
            i.image = texture;
            return i;
        }

        public Image AddImage<TTexture>(string path, IAssetLoader assetLoader, string? name = default, IEnumerable<string>? additionalClasses = default) where TTexture : Texture
        {
            var texture = assetLoader.Load<TTexture>(path);
            return parent.AddImage(texture, name, additionalClasses);
        }

        public Image AddImage(string path, IAssetLoader assetLoader, string? name = default, IEnumerable<string>? additionalClasses = default)
            => parent.AddImage<Texture>(path, assetLoader, name, additionalClasses);

        public Image AddImage(string iconName, NamedIconProvider provider, string? name = default, IEnumerable<string>? additionalClasses = default)
            => parent.AddImage(provider[iconName], name, additionalClasses);

        public Image AddInventoryIoImage(bool isOutput)
        {
            return AddImage(parent, additionalClasses: isOutput ? InventoryOutputClasses : InventoryInputClasses);
        }

        public Image AddInventoryInputImage() => AddInventoryIoImage(parent, false);
        public Image AddInventoryOutputImage() => AddInventoryIoImage(parent, true);

    }


}