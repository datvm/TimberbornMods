global using UiBuilder;
using System.Xml.Linq;

namespace UnityEngine.UIElements;

public static class UiBuilderExtensions
{

    public static TElement AddChild<TElement>(this VisualElement parent, string? name = default, IEnumerable<string>? classes = default)
        where TElement : VisualElement, new()
    {
        return (TElement)parent.AddChild(typeof(TElement), name, classes);
    }

    public static VisualElement AddChild(this VisualElement parent, Type type, string? name = default, IEnumerable<string>? classes = default)
    {
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

    public static readonly ImmutableArray<string> DefaultButtonClasses = ["unity-text-element", "unity-button",];
    public static NineSliceButton AddButton(this VisualElement parent, string text, string? name = default, IEnumerable<string>? additionalClasses = default, Action? onClick = default, GameButtonStyle style = GameButtonStyle.Menu, GameButtonSize size = GameButtonSize.Medium)
    {
        var btnClasses = GetClasses(style, size);

        var btn = parent.AddChild<NineSliceButton>(name, [..DefaultButtonClasses, .. btnClasses,.. (additionalClasses ?? [])]);
        btn.text = text;

        if (onClick is not null)
        {
            btn.clicked += onClick;
        }

        return btn;
    }

    public static readonly ImmutableArray<string> LabelClasses = ["unity-text-element", "unity-label"];
    public static Label AddLabel(this VisualElement parent, string text, string? name = default, IEnumerable<string>? additionalClasses = default, GameLabelStyle style = GameLabelStyle.Default)
    {
        var labelClasses = GetClasses(style);

        var label = parent.AddChild<Label>(name, [..LabelClasses, ..labelClasses, .. (additionalClasses ?? [])]);
        label.text = text;
        return label;
    }

    public static TElement AddClass<TElement>(this TElement element, string className) where TElement : VisualElement
    {
        element.classList.Add(className);
        return element;
    }

    public static TElement AddClasses<TElement>(this TElement element, params string[] classNames) where TElement : VisualElement
    {
        element.classList.AddRange(classNames);
        return element;
    }

    public static TElement SetMargin<TElement>(this TElement element, float margin) where TElement : VisualElement => element.SetMargin(margin, margin);

    public static TElement SetMargin<TElement>(this TElement element, float marginX = 0, float marginY = 0) where TElement : VisualElement => element.SetMargin(marginY, marginX, marginY, marginX);

    public static TElement SetMargin<TElement>(this TElement element, float top = 0, float right = 0, float bottom = 0, float left = 0) where TElement : VisualElement
    {
        element.style.marginTop = top;
        element.style.marginRight = right;
        element.style.marginBottom = bottom;
        element.style.marginLeft = left;
        return element;
    }

    public static TElement SetSize<TElement>(this TElement element, float? width = default, float? height = default) where TElement : VisualElement
    {
        if (width is not null)
        {
            element.style.width = width.Value;
        }

        if (height is not null)
        {
            element.style.height = height.Value;
        }

        return element;
    }

    public static TElement SetSize<TElement>(this TElement element, float widthAndHeight) where TElement : VisualElement => element.SetSize(widthAndHeight, widthAndHeight);

    public static TElement SetAsRow<TElement>(this TElement element) where TElement : VisualElement
    {
        element.style.flexDirection = FlexDirection.Row;
        return element;
    }

    public static string[] GetClasses(GameLabelStyle style) => style switch
    {
        GameLabelStyle.Default => ["text--default"],
        GameLabelStyle.Header => ["text--header"],
        _ => [],
    };

    public static string[] GetClasses(GameButtonStyle style, GameButtonSize size)
    {
        var styleClass = style switch
        {
            GameButtonStyle.Menu => "menu-button",
            _ => ""
        };

        var sizeClass = size switch
        {
            GameButtonSize.Small => "menu-button--small",
            GameButtonSize.Medium => "menu-button--medium",
            GameButtonSize.Large => "menu-button--large",
            _ => ""
        };

        return [styleClass, sizeClass];
    }


}