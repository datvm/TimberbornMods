namespace DynamicTailsBanners.Components;

[AddTemplateModule2(typeof(DynamicBuildingDecal))]
public class DynamicBannerTextRenderer(TextTextureRenderer renderer) : BaseComponent, IAwakableComponent
{
    const int Size = 256;

    TextTexture? textTexture;
    public Texture2D? Texture => textTexture?.Texture;

#nullable disable
    DynamicDecalOption optionsComp;
#nullable enable

    DynamicBannerTextOptions? opts;

    public void Awake()
    {
        optionsComp = GetComponent<DynamicDecalOption>();
    }

    public void SetContent(string? text)
        => SetContent(text, TextTextureRenderOptions.Default);

    public void SetContent(string? text, TextTextureRenderOptions options)
    {
        if (textTexture is null)
        {
            throw new InvalidOperationException($"Text texture is not initialized. Call {nameof(InitializeText)} first.");
        }

        text ??= "?";

        // No need caching here, TextTextureRenderer already does that internally.
        textTexture.Render(text, options);
    }

    public void Enable()
    {
        Disable();

        opts = optionsComp.GetSettingsOrThrow<DynamicBannerTextOptions>();
        if (opts.FontName is null)
        {
            throw new InvalidOperationException($"Font name is not set in options. Cannot enable DynamicBannerTextRenderer.");
        }

        SetFont(opts.FontName, opts.FontSize);
    }

    public void SetFont(string fontName, int fontSize)
    {
        InitializeText(fontName, fontSize);
        SetContent(opts!.Content);

        this.RefreshDecalTexture();
    }

    public void Disable()
    {
        opts = null;
        DisposeText();
    }

    void InitializeText(string fontName, int fontSize)
    {
        if (textTexture is not null)
        {
            DisposeText();
        }

        textTexture = renderer.Create(Size, Size, [fontName], fontSize);
    }

    void DisposeText()
    {
        textTexture?.Dispose();
        textTexture = null;
    }

}
