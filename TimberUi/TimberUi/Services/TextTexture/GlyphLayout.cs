namespace TimberUi.Services;

public static class GlyphLayout
{
    public static TextGlyphLayout Layout(
        string content,
        TextGlyphCache cache,
        IReadOnlyDictionary<char, TextGlyphCacheEntry> glyphs,
        int width,
        int height,
        TextTextureRenderOptions options)
    {
        if (string.IsNullOrEmpty(content))
        {
            return new TextGlyphLayout
            {
                Lines = [],
                Glyphs = [],
                Size = Vector2Int.zero
            };
        }

        var rawLines = BuildRawLines(content, cache, glyphs, width, options);
        var lineHeight = Mathf.RoundToInt(cache.LineHeight);

        var textWidth = rawLines.Count == 0 ? 0 : rawLines.Max(l => l.Width);

        var topBearing = 0;
        var bottomBearing = 0;

        foreach (var rawLine in rawLines)
        {
            foreach (var item in rawLine.Items)
            {
                if (!glyphs.TryGetValue(item.Character, out var entry))
                {
                    continue;
                }

                var info = entry.CharacterInfo;

                topBearing = Mathf.Max(topBearing, info.maxY);
                bottomBearing = Mathf.Max(bottomBearing, -info.minY);
            }
        }

        var textHeight = rawLines.Count == 0
            ? 0
            : (rawLines.Count - 1) * lineHeight + topBearing + bottomBearing;

        var startY = Align(height, textHeight, options.VerticalAlignment);
        var firstBaselineY = startY + topBearing;

        List<TextGlyphLayoutGlyph> finalGlyphs = [];
        List<TextGlyphLayoutLine> finalLines = [];

        for (var lineIndex = 0; lineIndex < rawLines.Count; lineIndex++)
        {
            var rawLine = rawLines[lineIndex];
            var startX = Align(width, rawLine.Width, options.HorizontalAlignment);
            var baselineY = firstBaselineY + lineIndex * lineHeight;

            var firstGlyphIndex = finalGlyphs.Count;
            var penX = startX;

            foreach (var item in rawLine.Items)
            {
                if (item.Character == TextTextureRenderer.Space ||
                    item.Character == TextTextureRenderer.Tab)
                {
                    penX += item.Advance;
                    continue;
                }

                if (!glyphs.TryGetValue(item.Character, out var entry))
                {
                    continue;
                }

                var info = entry.CharacterInfo;

                // Texture destination in top-left coordinates.
                var x = penX + info.minX;
                var y = baselineY - info.maxY;

                finalGlyphs.Add(new(
                    item.Character,
                    entry,
                    new Vector2Int(x, y),
                    item.Advance));

                penX += item.Advance;
            }

            finalLines.Add(new(
                firstGlyphIndex,
                finalGlyphs.Count - firstGlyphIndex,
                rawLine.Width,
                startY + lineIndex * lineHeight));
        }

        return new TextGlyphLayout
        {
            Lines = [.. finalLines],
            Glyphs = [.. finalGlyphs],
            Size = new Vector2Int(textWidth, textHeight)
        };
    }

    static List<RawLine> BuildRawLines(
        string content,
        TextGlyphCache cache,
        IReadOnlyDictionary<char, TextGlyphCacheEntry> glyphs,
        int width,
        TextTextureRenderOptions options)
    {
        Func<string, TextGlyphCache, IReadOnlyDictionary<char, TextGlyphCacheEntry>, int, TextTextureRenderOptions, List<RawLine>> wrapBuilder
            = options.WrapMode switch
            {
                TextTextureRenderWrapMode.None => BuildUnwrappedLines,
                TextTextureRenderWrapMode.Anywhere => BuildAnywhereWrappedLines,
                TextTextureRenderWrapMode.Space => BuildSpaceWrappedLines,
                _ => BuildUnwrappedLines
            };

        return wrapBuilder(content, cache, glyphs, width, options);
    }

    static List<RawLine> BuildUnwrappedLines(
        string content,
        TextGlyphCache cache,
        IReadOnlyDictionary<char, TextGlyphCacheEntry> glyphs,
        int _,
        TextTextureRenderOptions options)
    {
        List<RawLine> lines = [];
        var line = new RawLine();
        lines.Add(line);

        foreach (var c in content)
        {
            if (c == TextTextureRenderer.CarriageReturn)
            {
                continue;
            }

            if (c == TextTextureRenderer.NewLine)
            {
                line = new RawLine();
                lines.Add(line);
                continue;
            }

            var advance = GetAdvance(c, cache, glyphs, options);
            line.Items.Add(new(c, advance));
            line.Width += advance;
        }

        return lines;
    }

    static List<RawLine> BuildAnywhereWrappedLines(
        string content,
        TextGlyphCache cache,
        IReadOnlyDictionary<char, TextGlyphCacheEntry> glyphs,
        int width,
        TextTextureRenderOptions options)
    {
        List<RawLine> lines = [];
        var line = new RawLine();
        lines.Add(line);

        foreach (var c in content)
        {
            if (c == TextTextureRenderer.CarriageReturn)
            {
                continue;
            }

            if (c == TextTextureRenderer.NewLine)
            {
                line = new RawLine();
                lines.Add(line);
                continue;
            }

            var advance = GetAdvance(c, cache, glyphs, options);

            if (line.Width > 0 && line.Width + advance > width)
            {
                line = new RawLine();
                lines.Add(line);
            }

            line.Items.Add(new(c, advance));
            line.Width += advance;
        }

        return lines;
    }

    static List<RawLine> BuildSpaceWrappedLines(
        string content,
        TextGlyphCache cache,
        IReadOnlyDictionary<char, TextGlyphCacheEntry> glyphs,
        int width,
        TextTextureRenderOptions options)
    {
        List<RawLine> lines = [];
        var line = new RawLine();
        lines.Add(line);

        List<RawItem> word = [];
        var wordWidth = 0;

        void FlushWord()
        {
            if (word.Count == 0)
            {
                return;
            }

            var isWhitespaceWord =
                word.Count == 1 &&
                (word[0].Character == TextTextureRenderer.Space ||
                 word[0].Character == TextTextureRenderer.Tab);

            if (wordWidth <= width)
            {
                if (line.Width > 0 && line.Width + wordWidth > width)
                {
                    TrimTrailingSpaces(line);

                    line = new RawLine();
                    lines.Add(line);

                    TrimLeadingSpaces(word, ref wordWidth);
                }

                line.Items.AddRange(word);
                line.Width += wordWidth;
            }
            else if (!isWhitespaceWord)
            {
                TrimLeadingSpaces(word, ref wordWidth);

                foreach (var wordItem in word)
                {
                    if (line.Width > 0 && line.Width + wordItem.Advance > width)
                    {
                        TrimTrailingSpaces(line);

                        line = new RawLine();
                        lines.Add(line);
                    }

                    line.Items.Add(wordItem);
                    line.Width += wordItem.Advance;
                }
            }

            word.Clear();
            wordWidth = 0;
        }

        foreach (var c in content)
        {
            if (c == TextTextureRenderer.CarriageReturn)
            {
                continue;
            }

            if (c == TextTextureRenderer.NewLine)
            {
                FlushWord();
                TrimTrailingSpaces(line);

                line = new RawLine();
                lines.Add(line);
                continue;
            }

            var advance = GetAdvance(c, cache, glyphs, options);
            var item = new RawItem(c, advance);

            word.Add(item);
            wordWidth += advance;

            if (c == TextTextureRenderer.Space || c == TextTextureRenderer.Tab)
            {
                FlushWord();
            }
        }

        FlushWord();
        TrimTrailingSpaces(line);

        return lines;
    }

    static int GetAdvance(
        char c,
        TextGlyphCache cache,
        IReadOnlyDictionary<char, TextGlyphCacheEntry> glyphs,
        TextTextureRenderOptions options)
    {
        return c switch
        {
            TextTextureRenderer.Space => cache.SpaceWidth,
            TextTextureRenderer.Tab => cache.SpaceWidth * options.TabSize,
            _ => glyphs.TryGetValue(c, out var glyph)
                ? glyph.CharacterInfo.advance
                : 0
        };
    }

    static int Align(
        int available,
        int used,
        TextTextureRenderAlignment alignment)
    {
        return alignment switch
        {
            TextTextureRenderAlignment.Start => 0,
            TextTextureRenderAlignment.Center => (available - used) / 2,
            TextTextureRenderAlignment.End => available - used,
            _ => 0
        };
    }

    static void TrimTrailingSpaces(RawLine line)
    {
        while (line.Items.Count > 0)
        {
            var last = line.Items[^1];

            if (last.Character != TextTextureRenderer.Space &&
                last.Character != TextTextureRenderer.Tab)
            {
                return;
            }

            line.Items.RemoveAt(line.Items.Count - 1);
            line.Width -= last.Advance;
        }
    }

    static void TrimLeadingSpaces(List<RawItem> items, ref int width)
    {
        while (items.Count > 0)
        {
            var first = items[0];

            if (first.Character != TextTextureRenderer.Space &&
                first.Character != TextTextureRenderer.Tab)
            {
                return;
            }

            items.RemoveAt(0);
            width -= first.Advance;
        }
    }

    sealed class RawLine
    {
        public List<RawItem> Items { get; } = [];
        public int Width { get; set; }
    }

    readonly record struct RawItem(char Character, int Advance);
}