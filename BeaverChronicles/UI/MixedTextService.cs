namespace BeaverChronicles.UI;

[BindSingleton]
public class MixedTextService(
    IGoodService goodService,
    NamedIconProvider namedIconProvider
)
{
    public const string Opening = "[[";
    public const string Closing = "]]";
    public const char PlaceholderSeparator = ':';
    static readonly string[] LineSeparators = ["\r\n", "\n", "\r"];

    readonly record struct ParsedPart(VisualElement? Element, PartControl Control = PartControl.None);

    readonly VisualElement tmp = new();

    public static IEnumerable<string> GetParts(string content) => content.Split(LineSeparators, StringSplitOptions.None);

    static IEnumerable<(string Content, bool IsPlaceholder)> GetInlineParts(string content)
    {
        var curr = 0;

        while (curr < content.Length)
        {
            var opening = content.IndexOf(Opening, curr, StringComparison.Ordinal);
            if (opening == -1)
            {
                yield return (content[curr..], false);
                yield break;
            }

            var closing = content.IndexOf(Closing, opening + Opening.Length, StringComparison.Ordinal);
            if (closing == -1)
            {
                yield return (content[curr..], false);
                yield break;
            }

            if (opening > curr)
            {
                yield return (content[curr..opening], false);
            }

            yield return (content[(opening + Opening.Length)..closing], true);

            curr = closing + Closing.Length;
        }
    }

    public IEnumerable<VisualElement> CreateElements(string content)
    {
        foreach (var line in GetParts(content))
        {
            var lineEl = new VisualElement().SetAsRow().AlignItems().SetWrap();

            Stack<TimberbornTextColor> colorStack = [];
            TimberbornTextColor? curr = null;

            var firstInLine = true;

            foreach (var part in GetInlineParts(line))
            {
                var parsedPart = CreatePart(part);
                var partEl = parsedPart.Element;

                switch (parsedPart.Control)
                {
                    case PartControl.PushPositiveColor:
                        colorStack.Push(TimberbornTextColor.Green);
                        curr = TimberbornTextColor.Green;
                        break;
                    case PartControl.PushNegativeColor:
                        colorStack.Push(TimberbornTextColor.Red);
                        curr = TimberbornTextColor.Red;
                        break;
                    case PartControl.PopColor:
                        if (colorStack.Count > 0)
                        {
                            colorStack.Pop();
                        }
                        curr = colorStack.Count > 0 ? colorStack.Peek() : null;
                        break;
                    case PartControl.CenterLine:
                        lineEl.JustifyContent();
                        break;
                    default:
                        if (partEl is null) { continue; }

                        if (curr is not null)
                        {
                            partEl.HighlightAll(curr.Value);
                        }

                        if (firstInLine)
                        {
                            firstInLine = false;
                        }
                        else
                        {
                            partEl.SetMargin(left: 5);
                        }

                        lineEl.Add(partEl);
                        break;
                }
            }

            yield return lineEl;
        }
    }

    ParsedPart CreatePart((string, bool) content)
    {
        var (text, isPlaceholder) = content;
        if (!isPlaceholder)
        {
            return new ParsedPart(tmp.AddGameLabel(TextColors.ColorizeText(text)));
        }

        var parameters = text.Split(PlaceholderSeparator);
        if (parameters.Length == 0) { return default; }

        parameters[0] = parameters[0].Trim().ToUpperInvariant();
        TryCreatePart? method = parameters[0] switch
        {
            "GOOD" => TryCreateGoodPart,
            "ICON" => TryCreateIconPart,
            "TIME" => TryCreateTimePart,
            "+" or "-" or "/" or "C" => TryCreateControlPart,
            _ => null,
        };

        try
        {
            if (method is not null && method(parameters, out var part))
            {
                return part;
            }

            return CreatePart(($"<RedHighlight>Unparsable: {text}</RedHighlight>", false));
        }
        catch (InvalidOperationException ex)
        {
            return CreatePart(($"<RedHighlight>Unparsable: {text}. {ex.Message}</RedHighlight>", false));
        }

    }

    delegate bool TryCreatePart(string[] parameters, out ParsedPart part);

    bool TryCreateGoodPart(string[] parameters, out ParsedPart part)
    {
        part = default;
        if (parameters.Length < 2) { return false; }

        var goodId = parameters[1].Trim();
        var isScience = goodId == ActiveEventPayment.ScienceId;

        if (!isScience && !goodService.HasGood(goodId))
        {
            throw new InvalidOperationException($"No good ID: {goodId}");
        }

        string? amount = null;
        TimberbornTextColor? color = null;
        if (parameters.Length > 2)
        {
            amount = parameters[2].Trim();
            switch (amount[0])
            {
                case '-':
                    color = TimberbornTextColor.Red;
                    break;
                case '+':
                    color = TimberbornTextColor.Green;
                    break;
            }
        }

        var el = new IconSpan();
        
        if (isScience)
        {
            el.SetScience(namedIconProvider, amount).SetVertical(false);
        }
        else
        {
            el.SetGood(goodService, goodId, amount, true);
        }
        

        if (color is not null)
        {
            el.HighlightAll(color.Value);
        }

        part = new ParsedPart(el);

        return true;
    }

    bool TryCreateTimePart(string[] parameters, out ParsedPart part)
    {
        part = default;
        if (parameters.Length < 2) { return false; }
        
        var time = parameters[1].Trim();        
        var el = new IconSpan().SetTime(namedIconProvider, time).SetVertical(false);

        part = new ParsedPart(el);
        return true;
    }

    bool TryCreateIconPart(string[] parameters, out ParsedPart part)
    {
        part = default;
        if (parameters.Length < 2) { return false; }

        var path = parameters[1].Trim();
        var sprite = namedIconProvider.GetOrLoad(path, path);

        var img = new Image().SetSize(24);
        img.sprite = sprite;

        part = new ParsedPart(img);

        return true;
    }

    bool TryCreateControlPart(string[] parameters, out ParsedPart part)
    {
        var control = parameters[0] switch
        {
            "+" => PartControl.PushPositiveColor,
            "-" => PartControl.PushNegativeColor,
            "/" => PartControl.PopColor,
            "C" => PartControl.CenterLine,
            _ => PartControl.None
        };

        part = new ParsedPart(null, control);
        return control != PartControl.None;
    }

    enum PartControl
    {
        None,
        PushPositiveColor,
        PushNegativeColor,
        PopColor,
        CenterLine,
    }
}
