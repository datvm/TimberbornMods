namespace TimberUi.Services;

public class TextGlyphCache(Font font, int fontSize) : IDisposable
{

    public Font Font => font;
    public FontDefinitionPair FontDefinition { get; } = new(font.name, fontSize);
    public string FontName => FontDefinition.Name;
    public int FontSize => FontDefinition.Size;
    public int LineHeight => font.lineHeight;
    public int SpaceWidth => glyphs[' '].CharacterInfo.advance;
    internal Dictionary<char, TextGlyphCacheEntry> glyphs = [];

    internal int cacheUse = 0;

    public IEnumerable<char> GetMissingGlyphs(IEnumerable<char> glyphs)
    {
        foreach (var c in glyphs)
        {
            if (!this.glyphs.ContainsKey(c))
            {
                yield return c;
            }
        }
    }

    public void Dispose()
    {
        foreach (var v in glyphs.Values)
        {
            Object.Destroy(v.Texture);
        }

        Object.Destroy(font);
    }

}
