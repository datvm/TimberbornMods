namespace UnityEngine.UIElements;

public static class UIHelpers
{
    public static Color BorderColor = ColorUtility.TryParseHtmlString("#907B4A", out var c) ? c : Color.yellow;


    extension<T>(T ve) where T : VisualElement
    {

        public T SetBorder() => ve
            .SetBorder(BorderColor, 1)
            .SetPadding(5);

    }

}
