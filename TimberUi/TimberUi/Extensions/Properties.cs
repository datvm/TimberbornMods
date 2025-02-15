namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

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

}