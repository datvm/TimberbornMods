namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

    public static T AddClass<T>(this T element, string className) where T : VisualElement
    {
        element.classList.Add(className);
        return element;
    }

    public static T AddClasses<T>(this T element, params IEnumerable<string> classNames) where T : VisualElement
    {
        element.classList.AddRange(classNames);
        return element;
    }

    public static T SetMargin<T>(this T element, float margin) where T : VisualElement => element.SetMargin(margin, margin);

    public static T SetMargin<T>(this T element, float marginX = 0, float marginY = 0) where T : VisualElement => element.SetMargin(marginY, marginX, marginY, marginX);

    public static T SetMargin<T>(this T element, float top = 0, float right = 0, float bottom = 0, float left = 0) where T : VisualElement
    {
        element.style.marginTop = top;
        element.style.marginRight = right;
        element.style.marginBottom = bottom;
        element.style.marginLeft = left;
        return element;
    }

    public static T SetMarginBottom<T>(this T element, float margin = 20f) where T : VisualElement
    {
        element.style.marginBottom = margin;
        return element;
    }

    public static T SetMarginRight<T>(this T element, float margin = 20f) where T : VisualElement
    {
        element.style.marginRight = margin;
        return element;
    }

    public static T SetPadding<T>(this T element, float padding) where T : VisualElement => element.SetPadding(padding, padding);

    public static T SetPadding<T>(this T element, float paddingX = 0, float paddingY = 0) where T : VisualElement => element.SetPadding(paddingY, paddingX, paddingY, paddingX);

    public static T SetPadding<T>(this T element, float top = 0, float right = 0, float bottom = 0, float left = 0) where T : VisualElement
    {
        element.style.paddingTop = top;
        element.style.paddingRight = right;
        element.style.paddingBottom = bottom;
        element.style.paddingLeft = left;
        return element;
    }

    public static T SetSize<T>(this T element, float? width = default, float? height = default) where T : VisualElement
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

    public static T SetSize<T>(this T element, float widthAndHeight) where T : VisualElement => element.SetSize(widthAndHeight, widthAndHeight);

    public static T SetAsRow<T>(this T element) where T : VisualElement
    {
        element.style.flexDirection = FlexDirection.Row;
        return element;
    }

    public static T SetWrap<T>(this T element, bool wrap = true) where T : VisualElement
    {
        element.style.flexWrap = wrap ? Wrap.Wrap : Wrap.NoWrap;
        return element;
    }

    public static T SetMaxHeight<T>(this T element, float maxHeight) where T : VisualElement
    {
        element.style.maxHeight = maxHeight;
        return element;
    }

    public static T SetFlexGrow<T>(this T element, float flexGrow = 1) where T : VisualElement
    {
        element.style.flexGrow = flexGrow;
        return element;
    }

    public static T AddLabelClasses<T>(this T element, GameLabelStyle style, GameLabelSize size = default, GameLabelColor? color = default, bool bold = default) where T : TextElement
    {
        element.classList.AddRange(GetClasses(style, size, color, bold));
        return element;
    }

    public static IDropdownProvider SetItems<T>(this T dropdown, DropdownItemsSetter setter, IReadOnlyList<string> list, string? defaultValue) where T : Dropdown
    {
        var provider = new SimpleDropdownItemProvider(list, defaultValue);
        setter.SetItems(dropdown, provider);

        return provider;
    }

    public static string? GetSelectedValue<T>(this T dropdown) where T : Dropdown
    {
        return dropdown._dropdownProvider?.GetValue();
    }

    public static int GetSelectedIndex<T>(this T dropdown) where T : Dropdown
    {
        if (dropdown._dropdownProvider is null) { return -1; }

        return dropdown._dropdownProvider.Items.IndexOf(dropdown._dropdownProvider.GetValue());
    }

}