namespace ConfigurableTopBar.Helpers;

public static class ModHelpers
{

    public static void AddMoveButton(this VisualElement parent, bool up, Action<bool> onClick)
    {
        parent.AddGameButton(
            up ? "↑" : "↓",
            onClick: () => onClick(up),
            additionalClasses: [UiCssClasses.ButtonSquare,]
        ).SetMarginRight(3);
    }

}
