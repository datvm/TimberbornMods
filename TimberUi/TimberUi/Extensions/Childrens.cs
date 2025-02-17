
namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

    public static T AddChild<T>(this VisualElement parent, string? name = default, IEnumerable<string>? classes = default)
        where T : VisualElement, new()
    {
        return (T)parent.AddChild(typeof(T), name, classes);
    }

    public static VisualElement AddChild(this VisualElement parent, Type? type = default, string? name = default, IEnumerable<string>? classes = default)
    {
        type ??= typeof(VisualElement);

        var child = Activator.CreateInstance(type) as VisualElement
            ?? throw new InvalidOperationException("Failed to create VisualElement instance.");

        parent.Add(child);

        if (name is not null)
        {
            child.name = name;
        }

        if (classes is not null)
        {
            child.classList.AddRange(classes);
        }

        return child;
    }

    public static VisualElement InsertSelfAsSibling(this VisualElement element, VisualElement target, int delta)
    {
        var parent = target.parent;
        var index = parent.IndexOf(target);
        parent.Insert(index + delta, element);
        return element;
    }

    public static VisualElement InsertSelfBefore(this VisualElement element, VisualElement target) => element.InsertSelfAsSibling(target, 0);
    public static VisualElement InsertSelfAfter(this VisualElement element, VisualElement target) => element.InsertSelfAsSibling(target, 1);

    public static NineSliceButton AddButton(this VisualElement parent, string text, string? name = default, Action? onClick = default, IEnumerable<string>? additionalClasses = default, GameButtonStyle style = GameButtonStyle.Menu, GameButtonSize? size = default, bool stretched = false)
    {
        EventCallback<ClickEvent>? callback = onClick is null
            ? null
            : (_) => onClick();

        return AddButton(parent, text, callback, name, additionalClasses, style, size, stretched);
    }

    public static NineSliceButton AddButton(this VisualElement parent, string text, EventCallback<ClickEvent>? clickCb, string? name = default, IEnumerable<string>? additionalClasses = default, GameButtonStyle style = GameButtonStyle.Menu, GameButtonSize? size = default, bool stretched = false)
    {
        var btnClasses = GetClasses(style, size, stretched);

        var btn = parent.AddChild<NineSliceButton>(name, [.. btnClasses, .. (additionalClasses ?? [])]);
        btn.text = text;

        if (clickCb is not null)
        {
            btn.RegisterCallback(clickCb);
        }

        return btn;
    }

    public static NineSliceButton AddMenuButton(this VisualElement parent, string text, Action? onClick = default, string? name = default, IEnumerable<string>? additionalClasses = default, GameButtonSize? size = default, bool stretched = false)
    {
        return AddButton(parent, text, name, onClick, additionalClasses, GameButtonStyle.Menu, size, stretched);
    }

    public static Label AddLabel(this VisualElement parent, string text, string? name = default, IEnumerable<string>? additionalClasses = default, GameLabelStyle style = GameLabelStyle.Default)
    {
        var labelClasses = GetClasses(style);

        var label = parent.AddChild<Label>(name, [.. labelClasses, .. (additionalClasses ?? [])]);
        label.text = text;
        return label;
    }

    public static Label AddLabelHeader(this VisualElement parent, string text, string? name = default, IEnumerable<string>? additionalClasses = default)
        => parent.AddLabel(text, name, additionalClasses, GameLabelStyle.Header);

    public static ScrollView AddGameScrollView(this VisualElement parent, string? name = default, IEnumerable<string>? additionalClasses = default, bool greenDecorated = true)
    {
        if (greenDecorated)
        {
            additionalClasses = (additionalClasses ?? []).Append(UiCssClasses.ScrollGreenDecorated);
        }

        return parent.AddChild<ScrollView>(name, additionalClasses);
    }

    public static ListView AddGameListView(this VisualElement parent, string? name = default, IEnumerable<string>? additionalClasses = default, bool greenDecorated = true)
    {
        if (greenDecorated)
        {
            additionalClasses = (additionalClasses ?? []).Append(UiCssClasses.ScrollGreenDecorated);
        }
        return parent.AddChild<ListView>(name, additionalClasses);
    }

    public static string[] GetClasses(GameLabelStyle style) => style switch
    {
        GameLabelStyle.Default => ["text--default"],
        GameLabelStyle.Header => ["text--header"],
        _ => [],
    };

    public static IEnumerable<string> GetClasses(GameButtonStyle style, GameButtonSize? size = default, bool stretched = false)
    {
        List<string> result = [];

        var styleClass = style switch
        {
            GameButtonStyle.Text => null,
            GameButtonStyle.Menu => UiCssClasses.ButtonMenu,
            GameButtonStyle.WideMenu => UiCssClasses.ButtonWideMenu,
            _ => throw new NotImplementedException(style.ToString()),
        };

        if (styleClass is null)
        {
            result.AddRange(UiCssClasses.ButtonText);
        }
        else
        {
            result.Add(styleClass);
        }

        if (size is not null)
        {
            var sizeClass = styleClass + size switch
            {
                GameButtonSize.Medium => UiCssClasses.ButtonPfMedium,
                GameButtonSize.Large => UiCssClasses.ButtonPfLarge,
                _ => throw new NotImplementedException(size.ToString()),
            };
            result.Add(sizeClass);
        }

        if (stretched)
        {
            result.Add(styleClass + UiCssClasses.ButtonPfStretched);
        }

        return result;
    }


}