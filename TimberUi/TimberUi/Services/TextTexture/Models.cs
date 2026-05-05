namespace TimberUi.Services;

public readonly record struct FontDefinitionPair(string Name, int Size);

public enum TextTextureRenderWrapMode
{
    None = 0,
    Space = 1,
    Anywhere = 2,
}

public enum TextTextureRenderAlignment
{
    Start,
    Center,
    End,
}

public readonly record struct TextTextureRenderOptions(
    int TabSize = 4,
    TextTextureRenderWrapMode WrapMode = TextTextureRenderWrapMode.Space,
    TextTextureRenderAlignment HorizontalAlignment = TextTextureRenderAlignment.Center,
    TextTextureRenderAlignment VerticalAlignment = TextTextureRenderAlignment.Center
)
{
    public static readonly TextTextureRenderOptions Default = new(4);
}


public readonly record struct TextGlyphCacheEntry(char C, Texture2D Texture, CharacterInfo CharacterInfo);