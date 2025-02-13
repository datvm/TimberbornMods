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
    public static NineSliceButton AddButton(this VisualElement parent, string text, string? name = default, IEnumerable<string>? additionalClasses = default, Action? onClick = default)
    {
        var btn = parent.AddChild<NineSliceButton>(name, [..DefaultButtonClasses, .. (additionalClasses ?? [])]);
        btn.text = text;

        if (onClick is not null)
        {
            btn.clicked += onClick;
        }

        return btn;
    }

    public static readonly ImmutableArray<string> DefaultLabelClasses = ["unity-text-element", "unity-label"];
    public static Label AddLabel(this VisualElement parent, string text, string? name = default, IEnumerable<string>? additionalClasses = default)
    {
        var label = parent.AddChild<Label>(name, [..DefaultLabelClasses, .. (additionalClasses ?? [])]);
        label.text = text;
        return label;
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

    public static TElement SetSize<TElement>(this TElement element, float width, float height) where TElement : VisualElement
    {
        element.style.width = width;
        element.style.height = height;
        return element;
    }

    public static TElement SetSize<TElement>(this TElement element, float widthAndHeight) where TElement : VisualElement => element.SetSize(widthAndHeight, widthAndHeight);

    public static TElement SetAsRow<TElement>(this TElement element) where TElement : VisualElement
    {
        element.style.flexDirection = FlexDirection.Row;
        return element;
    }

}
