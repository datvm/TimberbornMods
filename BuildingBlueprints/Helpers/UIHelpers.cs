namespace UnityEngine.UIElements;

public static class UIHelpers
{
    public static Color BorderColor = ColorUtility.TryParseHtmlString("#907B4A", out var c) ? c : Color.yellow;

    public static async void RunAsyncVoid(Func<Task> task)
    {
        try
        {
            await task();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    extension<T>(T ve) where T : VisualElement
    {

        public T SetBorder() => ve
            .SetBorder(BorderColor, 1)
            .SetPadding(5);

    }

}
