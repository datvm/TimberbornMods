namespace TimberUi.Services;

public class TextTexture(int width, int height, FontDefinitionPair fontDef, TextTextureRenderer renderer) : IDisposable
{
    static readonly Dictionary<Vector2Int, Texture2D> clearTextures = [];

    public int Width => width;
    public int Height => height;
    public FontDefinitionPair FontDef => fontDef;
    public string FontName => fontDef.Name;
    public int FontSize => fontDef.Size;

    public bool Disposed { get; private set; }

    internal static void DisposeClearTextureCache()
    {
        foreach (var tex in clearTextures.Values)
        {
            Object.Destroy(tex);
        }
        clearTextures.Clear();
    }

    public string Content { get; private set; } = "";
    TextTextureRenderOptions? currOptions;

    public Texture2D Texture { get; } = CreateTexture(width, height);

    public Vector2Int MeasureUnbound(string content) => renderer.Measure(fontDef, content);

    public void Render(string content)
        => Render(content, TextTextureRenderOptions.Default);

    public void Render(string content, TextTextureRenderOptions options)
    {
        if (content == Content && currOptions == options) { return; }

        currOptions = options;

        Clear();

        if (string.IsNullOrEmpty(content))
        {
            return;
        }

        var cache = renderer.GetCache(fontDef);
        var glyphs = renderer.GetGlyphMap(fontDef, content);

        var layout = GlyphLayout.Layout(content, cache, glyphs, width, height, options);

        foreach (var glyph in layout.Glyphs)
        {
            StampGlyph(glyph);
        }

        Content = content;
    }

    public void Clear()
    {
        var clear = GetClearTexture(width, height);
        Graphics.CopyTexture(clear, Texture);
        Content = "";
        currOptions = default;
    }

    void StampGlyph(TextGlyphLayoutGlyph glyph)
    {
        var src = glyph.Entry.Texture;

        var dstX = glyph.Position.x;

        // GlyphLayout position is top-left based.
        // CopyTexture uses bottom-left texture coordinates.
        var dstY = height - glyph.Position.y - src.height;

        if (dstX >= width || dstY >= height || dstX + src.width <= 0 || dstY + src.height <= 0)
        {
            return;
        }

        // Clip if partially outside.
        var srcX = 0;
        var srcY = 0;
        var copyWidth = src.width;
        var copyHeight = src.height;

        if (dstX < 0)
        {
            srcX = -dstX;
            copyWidth += dstX;
            dstX = 0;
        }

        if (dstY < 0)
        {
            srcY = -dstY;
            copyHeight += dstY;
            dstY = 0;
        }

        if (dstX + copyWidth > width)
        {
            copyWidth = width - dstX;
        }

        if (dstY + copyHeight > height)
        {
            copyHeight = height - dstY;
        }

        if (copyWidth <= 0 || copyHeight <= 0)
        {
            return;
        }

        Graphics.CopyTexture(
            src, 0, 0,
            srcX, srcY, copyWidth, copyHeight,
            Texture, 0, 0,
            dstX, dstY);
    }

    static Texture2D CreateTexture(int width, int height)
    {
        var texture = new Texture2D(width, height, TextTextureRenderer.GlyphTextureFormat, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        texture.Apply(false, false);
        return texture;
    }

    static Texture2D GetClearTexture(int width, int height)
    {
        var key = new Vector2Int(width, height);

        if (clearTextures.TryGetValue(key, out var tex))
        {
            return tex;
        }

        tex = CreateTexture(width, height);

        var pixels = new Color32[width * height];
        tex.SetPixels32(pixels);
        tex.Apply(false, true);

        clearTextures.Add(key, tex);
        return tex;
    }

    public void Dispose()
    {
        if (Disposed) { return; }

        Disposed = true;
        Object.Destroy(Texture);
        renderer.OnTextTextureDisposed(this);
    }
}
