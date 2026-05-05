namespace TimberUi.Services;

public readonly record struct TextGlyphLayoutGlyph(
    char Character,
    TextGlyphCacheEntry Entry,
    Vector2Int Position,
    int Advance
);

public sealed class TextGlyphLayout
{
    public ImmutableArray<TextGlyphLayoutLine> Lines { get; init; }
    public ImmutableArray<TextGlyphLayoutGlyph> Glyphs { get; init; }
    public Vector2Int Size { get; init; }
}

public readonly record struct TextGlyphLayoutLine(
    int StartGlyphIndex,
    int GlyphCount,
    int Width,
    int Y
);