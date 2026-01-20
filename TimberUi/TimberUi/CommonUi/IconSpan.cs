namespace TimberUi.CommonUi;

public class IconSpan : VisualElement
{

    public Image Icon { get; }
    public Label? PrefixLabel { get; private set; }
    public Label? PostfixLabel { get; private set; }

    public IconSpan()
    {
        this.AlignItems().JustifyContent();
        this.SetAsRow();

        Icon = this.AddImage().SetMargin(marginX: 2);
    }

    public IconSpan SetVertical(bool vertical = true)
    {
        style.flexDirection = vertical ? FlexDirection.Column : FlexDirection.Row;
        return this;
    }

    public IconSpan SetPrefixText(string text)
    {
        if (PrefixLabel is null)
        {
            PrefixLabel = this.AddLabel();
            PrefixLabel.InsertSelfBefore(Icon);
        }

        PrefixLabel.text = text;
        return this;
    }

    public IconSpan SetImage(ImageSource src, int? size = null)
        => SetImage(src, size is null ? null : new Vector2(size.Value, size.Value));

    public IconSpan SetImage(ImageSource src, Vector2? size = null)
    {
        src.SetImage(Icon);

        if (size.HasValue)
        {
            var (x, y) = size.Value;
            Icon.SetSize(x, y);
        }

        return this;
    }

    public IconSpan SetPostfixText(string text)
    {
        PostfixLabel ??= this.AddLabel();
        PostfixLabel.text = text;
        return this;
    }

    public IconSpan SetContent(ImageSource src, string? prefixText = null, string? postfixText = null, int? size = null)
        => SetContent(src, prefixText, postfixText, size is null ? null : new Vector2(size.Value, size.Value));

    public IconSpan SetContent(ImageSource src, string? prefixText = null, string? postfixText = null, Vector2? size = null)
    {
        if (prefixText is not null)
        {
            SetPrefixText(prefixText);
        }
        SetImage(src, size);
        if (postfixText is not null)
        {
            SetPostfixText(postfixText);
        }
        return this;
    }

}

public readonly record struct ImageSource(Sprite? Sprite = null, Texture? Texture = null, VectorImage? VectorImage = null)
{
    public void SetImage(Image image)
    {
        if (Sprite is not null)
        {
            image.sprite = Sprite;
        }
        else if (Texture is not null)
        {
            image.image = Texture;
        }
        else if (VectorImage is not null)
        {
            image.vectorImage = VectorImage;
        }
        else
        {
            throw new InvalidOperationException("ImageContent does not contain any image data.");
        }
    }

    public static implicit operator ImageSource(Sprite sprite) => new(Sprite: sprite);
    public static implicit operator ImageSource(Texture texture) => new(Texture: texture);
    public static implicit operator ImageSource(VectorImage vectorImage) => new(VectorImage: vectorImage);
    public static implicit operator Sprite(ImageSource content) => content.Sprite ?? throw new InvalidCastException("ImageContent does not contain a Sprite.");
    public static implicit operator Texture(ImageSource content) => content.Texture ?? throw new InvalidCastException("ImageContent does not contain a Texture.");
    public static implicit operator VectorImage(ImageSource content) => content.VectorImage ?? throw new InvalidCastException("ImageContent does not contain a VectorImage.");
}
