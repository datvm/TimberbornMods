namespace TimberUi.Services;

[BindSingleton(Contexts = BindAttributeContext.Bootstrapper, Exported = true)]
public class TextTextureRenderer : IUnloadableSingleton
{
    public const char NewLine = '\n';
    public const char Tab = '\t';
    public const char CarriageReturn = '\r';
    public const char Space = ' ';
    public static readonly FrozenSet<char> IgnoredChars = ImmutableHelper.CreateFrozenSet([NewLine, Tab, CarriageReturn, Space]);

    public static readonly string[] MonospaceFonts = TextTextureFontService.MonospaceFonts;
    public static readonly string[] DefaultFonts = TextTextureFontService.DefaultFonts;

    public const TextureFormat GlyphTextureFormat = TextureFormat.RGBA32;

    readonly Dictionary<FontDefinitionPair, TextGlyphCache> caches = [];

    public TextTexture Create(int width, int height, string[] fontNames, int fontSize = 64)
    {
        var cache = GetFontDefinition(fontNames, fontSize);
        cache.cacheUse++;

        return new(width, height, cache.FontDefinition, this);
    }

    TextGlyphCache GetFontDefinition(string[] fontNames, int size)
    {
        var font = Font.CreateDynamicFontFromOSFont(fontNames, size);

        if (!font)
        {
            throw new ArgumentException($"Could not create font from names: {string.Join(", ", fontNames)}");
        }

        var def = new FontDefinitionPair(font.name, size);
        if (caches.TryGetValue(def, out var cached))
        {
            return cached;
        }

        TimberUiUtils.LogVerbose(() => $"Allocating new font cache for {font.name} at size {size}. Total in cache: {caches.Count}.");

        cached = new(font, size);
        CacheSpace(cached);
        caches.Add(def, cached);
        return cached;
    }

    public TextGlyphCache GetCache(FontDefinitionPair fontDef)
        => caches.TryGetValue(fontDef, out var cache)
            ? cache
            : throw new InvalidOperationException($"Font wasn't initialized: {fontDef}");

    void CacheSpace(TextGlyphCache cache)
    {
        RenderMissingGlyphs(cache, [' ']);
    }

    public IReadOnlyDictionary<char, TextGlyphCacheEntry> GetGlyphMap(FontDefinitionPair fontDef, string content)
    {
        var cache = GetCache(fontDef);
        var glyphs = GetUniqueGlyphs(content);

        var missingGlyphs = cache.GetMissingGlyphs(glyphs).ToArray();
        if (missingGlyphs.Length > 0)
        {
            RenderMissingGlyphs(cache, missingGlyphs);
        }

        return cache.glyphs;
    }

    public Vector2Int Measure(FontDefinitionPair fontDef, string content, float scale = 1f)
        => Measure(fontDef, content, TextTextureRenderOptions.Default, scale);

    public Vector2Int Measure(FontDefinitionPair fontDef, string content, TextTextureRenderOptions options, float scale = 1f)
    {
        if (string.IsNullOrEmpty(content)) { return Vector2Int.zero; }

        var cache = GetCache(fontDef);
        var glyphs = GetGlyphMap(fontDef, content);
        var x = 0;
        var y = 0;
        var maxX = 0;

        var lineHeight = Mathf.RoundToInt(cache.LineHeight * scale);
        var spaceWidth = Mathf.RoundToInt(cache.SpaceWidth * scale);

        foreach (var c in content)
        {
            switch (c)
            {
                case NewLine:
                    maxX = Mathf.Max(maxX, x);
                    x = 0;
                    y += lineHeight;
                    continue;
                case CarriageReturn:
                    continue;
                case Tab:
                    x += spaceWidth * options.TabSize;
                    continue;
                case ' ':
                    x += spaceWidth;
                    continue;
            }

            if (!glyphs.TryGetValue(c, out var glyph))
            {
                continue;
            }

            x += Mathf.RoundToInt(glyph.CharacterInfo.advance * scale);
        }

        maxX = Mathf.Max(maxX, x);

        return new(maxX, y + lineHeight);
    }

    void RenderMissingGlyphs(TextGlyphCache cache, ReadOnlySpan<char> glyphs)
    {
        var font = cache.Font;

        if (glyphs.Length == 0)
        {
            return;
        }

        var content = new string(glyphs);
        font.RequestCharactersInTexture(content, cache.FontSize, FontStyle.Normal);

        var atlas = font.material.mainTexture;

        if (!atlas)
        {
            throw new InvalidOperationException($"Font atlas is missing for font: {font.name}");
        }

        var atlasWidth = atlas.width;
        var atlasHeight = atlas.height;

        foreach (var c in glyphs)
        {
            if (!font.GetCharacterInfo(c, out var info, cache.FontSize, FontStyle.Normal))
            {
                Debug.LogWarning($"Could not get glyph info for '{c}' from font '{font.name}'.");
                continue;
            }

            var glyphWidth = Mathf.Max(0, info.glyphWidth);
            var glyphHeight = Mathf.Max(0, info.glyphHeight);

            if (glyphWidth <= 0 || glyphHeight <= 0)
            {
                cache.glyphs[c] = new(c, CreateEmptyGlyphTexture(font, c), info);
                continue;
            }

            var texture = RenderGlyphTexture(font, atlas, info, glyphWidth, glyphHeight, c);
            cache.glyphs[c] = new(c, texture, info);
        }
    }

    public static IEnumerable<char> GetUniqueGlyphs(string content)
    {
        HashSet<char> chars = [];
        foreach (var c in content)
        {
            if (!IgnoredChars.Contains(c) && chars.Add(c))
            {
                yield return c;
            }
        }
    }

    static Texture2D CreateEmptyGlyphTexture(Font font, char c)
    {
        var texture = new Texture2D(1, 1, GlyphTextureFormat, false)
        {
            name = $"{font.name}_{(int)c:X4}_empty",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
        };

        texture.SetPixel(0, 0, Color.clear);
        texture.Apply(false, true);

        return texture;
    }

    static Texture2D RenderGlyphTexture(Font font, Texture atlas, CharacterInfo info, int width, int height, char c)
    {
        var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
        var previous = RenderTexture.active;

        try
        {
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.clear);

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, width, 0, height);

            var material = font.material;
            material.mainTexture = atlas;
            material.SetPass(0);

            GL.Begin(GL.QUADS);
            GL.Color(Color.white);

            // Map the glyph's logical corners to the destination quad corners.
            // This preserves rotation/mirroring from the atlas packing correctly.
            GL.TexCoord2(info.uvBottomLeft.x, info.uvBottomLeft.y);
            GL.Vertex3(0, 0, 0);

            GL.TexCoord2(info.uvBottomRight.x, info.uvBottomRight.y);
            GL.Vertex3(width, 0, 0);

            GL.TexCoord2(info.uvTopRight.x, info.uvTopRight.y);
            GL.Vertex3(width, height, 0);

            GL.TexCoord2(info.uvTopLeft.x, info.uvTopLeft.y);
            GL.Vertex3(0, height, 0);

            GL.End();
            GL.PopMatrix();

            var readable = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = $"{font.name}_{(int)c:X4}_readable",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };

            readable.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            readable.Apply(false, false);

            var sourcePixels = readable.GetPixels();
            Object.Destroy(readable);

            var pixels = new Color[sourcePixels.Length];
            for (var i = 0; i < sourcePixels.Length; i++)
            {
                var source = sourcePixels[i];
                var alpha = Mathf.Max(source.a, source.r, source.g, source.b);
                pixels[i] = new Color(1f, 1f, 1f, alpha);
            }

            var texture = new Texture2D(width, height, GlyphTextureFormat, false)
            {
                name = $"{font.name}_{(int)c:X4}",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };

            texture.SetPixels(pixels);
            texture.Apply(false, true);

            return texture;
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
        }
    }

    internal void OnTextTextureDisposed(TextTexture texture)
    {
        var cached = GetCache(texture.FontDef);
        cached.cacheUse--;

        if (cached.cacheUse <= 0)
        {
            TimberUiUtils.LogVerbose(() => $"Disposing font cache for {cached.Font.name} at size {cached.FontSize}. Total in cache: {caches.Count - 1}.");
            cached.Dispose();
            caches.Remove(texture.FontDef);
        }
    }

    public void Unload()
    {
        TextTexture.DisposeClearTextureCache();

        foreach (var cache in caches.Values)
        {
            cache.Dispose();
        }

        caches.Clear();
    }
}
