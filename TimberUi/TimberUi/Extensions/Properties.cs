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

    public static T SetMarginLeftAuto<T>(this T element) where T : VisualElement
    {
        element.style.marginLeft = new StyleLength(StyleKeyword.Auto);
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

    public static T SetWidth<T>(this T element, float width) where T : VisualElement => element.SetSize(width, null);
    public static T SetHeight<T>(this T element, float height) where T : VisualElement => element.SetSize(null, height);
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

    public static T SetMaxWidth<T>(this T element, float maxWidth) where T : VisualElement
        => element.SetMaxSize(maxWidth, null);

    public static T SetMaxHeight<T>(this T element, float maxHeight) where T : VisualElement
        => element.SetMaxSize(null, maxHeight);

    public static T SetMaxSize<T>(this T element, float maxWH) where T : VisualElement
        => element.SetMaxSize(maxWH, maxWH);

    public static T SetMaxSize<T>(this T element, float? maxW, float? maxH) where T : VisualElement
    {
        if (maxW is not null)
        {
            element.style.maxWidth = maxW.Value;
        }
        if (maxH is not null)
        {
            element.style.maxHeight = maxH.Value;
        }

        return element;
    }

    public static T SetMaxSizePercent<T>(this T element, float? maxW, float? maxH) where T : VisualElement
    {
        var s = element.style;
        if (maxW is not null)
        {
            s.maxWidth = new Length(maxW.Value, LengthUnit.Percent);
        }
        if (maxH is not null)
        {
            s.maxHeight = new Length(maxH.Value, LengthUnit.Percent);
        }
        return element;
    }

    public static T SetMinSize<T>(this T element, float minWH) where T : VisualElement
        => element.SetMinSize(minWH, minWH);

    public static T SetMinSize<T>(this T element, float? minW, float? minH) where T : VisualElement
    {
        if (minW is not null)
        {
            element.style.minWidth = minW.Value;
        }
        if (minH is not null)
        {
            element.style.minHeight = minH.Value;
        }
        return element;
    }

    public static T SetMinMaxSize<T>(this T element, float? w, float? h) where T : VisualElement
    {
        var s = element.style;

        if (w is not null)
        {
            s.minWidth = s.maxWidth = s.width = w.Value;
        }
        if (h is not null)
        {
            s.minHeight = s.maxHeight = s.height = h.Value;
        }
        return element;
    }

    public static T SetMinMaxSizePercent<T>(this T element, float? w, float? h) where T : VisualElement
    {
        var s = element.style;
        if (w is not null)
        {
            s.minWidth = s.maxWidth = s.width = new Length(w.Value, LengthUnit.Percent);
        }
        if (h is not null)
        {
            s.minHeight = s.maxHeight = s.height = new Length(h.Value, LengthUnit.Percent);
        }
        return element;
    }

    public static T SetWidthPercent<T>(this T element, float percent) where T : VisualElement
        => element.SetSizePercent(percent, null);

    public static T SetHeightPercent<T>(this T element, float percent) where T : VisualElement
        => element.SetSizePercent(null, percent);

    public static T SetSizePercent<T>(this T element, float? w, float? h) where T : VisualElement
    {
        var s = element.style;
        if (w is not null)
        {
            s.width = new Length(w.Value, LengthUnit.Percent);
        }
        if (h is not null)
        {
            s.height = new Length(h.Value, LengthUnit.Percent);
        }
        return element;
    }

    public static T SetFlexGrow<T>(this T element, float flexGrow = 1) where T : VisualElement
    {
        element.style.flexGrow = flexGrow;
        return element;
    }

    public static T SetFlexShrink<T>(this T element, float flexShrink = 1) where T : VisualElement
    {
        element.style.flexShrink = flexShrink;
        return element;
    }

    public static T SetDisplay<T>(this T element, bool display) where T : VisualElement
    {
        element.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
        return element;
    }

    public static T AlignItems<T>(this T element, Align align = Align.Center) where T : VisualElement
    {
        element.style.alignItems = align;
        return element;
    }

    public static T JustifyContent<T>(this T element, Justify justify = Justify.Center) where T : VisualElement
    {
        element.style.justifyContent = justify;
        return element;
    }

    public static T AddLabelClasses<T>(this T element, GameLabelStyle style, GameLabelSize size = default, GameLabelColor? color = default, bool bold = default) where T : TextElement
    {
        element.classList.AddRange(GetClasses(style, size, color, bold));
        return element;
    }

}