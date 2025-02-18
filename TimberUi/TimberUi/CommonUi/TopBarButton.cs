namespace TimberUi.CommonUi;

public class TopBarButton : NineSliceButton
{

    public TopBarButtonBackground Background
    {
        get;
        set
        {
            classList.Remove(GetBackgroundClass(field));
            field = value;
            classList.Add(GetBackgroundClass(value));
        }
    }

    public static string GetBackgroundClass(TopBarButtonBackground background)
    {
        return UiCssClasses.ButtonTopBarPrefix + background switch
        {
            TopBarButtonBackground.Green => UiCssClasses.Green,
            TopBarButtonBackground.Brown => UiCssClasses.Brown,
            _ => throw new ArgumentOutOfRangeException(nameof(background), background, null)
        };
    }

}

public enum TopBarButtonBackground
{
    Green,
    Brown,
}