using Timberborn.MainMenuPanels;
using UiBuilder.CommonUi;

namespace TimberUiDemo.Services;

public class MenuService(PanelStack stack, MainMenuPanel menu) : ILoadableSingleton
{

    public void Load()
    {
        Debug.Log("MenuService loaded");

        var btnExit = menu.GetExitGameButton();
        btnExit.AddMenuButton("Show demo dialog", ShowDialog, name: "ShowDemoDialog", stretched: true)
            .InsertSelfAfter(btnExit);

        menu._root.PrintVisualTree();
    }

    void ShowDialog()
    {
        var diag = new DialogBoxElement()
            .SetTitle("Dialog title")
            .AddCloseButton();

        diag.Content.AddLabel("Hello there!", style: UiBuilder.GameLabelStyle.Header);
        diag.Content.AddLabel("This is the content of the dialog");

        diag
            .PrintVisualTree()
            .Show(stack);
    }

}
