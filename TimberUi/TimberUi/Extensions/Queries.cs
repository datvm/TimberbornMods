namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

    public static NineSliceVisualElement GetMainMenuPanel(this MainMenuPanel panel)
        => panel._root.Q<NineSliceVisualElement>("MainMenuPanel");

    public static LocalizableButton GetExitGameButton(this MainMenuPanel panel)
        => panel._root.Q<LocalizableButton>("ExitButton");

}