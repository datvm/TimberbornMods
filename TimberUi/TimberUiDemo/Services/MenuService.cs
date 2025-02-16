global using Timberborn.MainMenuPanels;
global using UiBuilder.CommonUi;
global using UiBuilder;

namespace TimberUiDemo.Services;

public class MenuService(PanelStack stack, MainMenuPanel menu, VisualElementLoader veLoader, DialogBoxShower diagShower) : ILoadableSingleton
{

    static readonly string[] Assets = [];

    void PrintDebugInfo()
    {
        foreach (var a in Assets)
        {
            veLoader.LoadVisualElement(a).PrintVisualTree();
        }
    }

    public void Load()
    {
        PrintDebugInfo();

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

        diag.Content.AddLabelHeader("Label: Header");
        diag.Content.AddLabel("Label: Default")
            .SetMargin(bottom: 10);

        diag.Content.AddLabelHeader("Buttons");
        
        var menuBtns = diag.Content.AddChild(name: "MenuButtons").SetAsRow();
        {
            menuBtns.AddMenuButton("Menu button", OnButtonClicked);
            menuBtns.AddMenuButton("Menu button - Small", OnButtonClicked, size: GameButtonSize.Small);
            menuBtns.AddMenuButton("Menu button - Medium", OnButtonClicked, size: GameButtonSize.Medium);
            menuBtns.AddMenuButton("Menu button - Large", OnButtonClicked, size: GameButtonSize.Large);
            menuBtns.AddMenuButton("Menu button - Stretched", OnButtonClicked, stretched: true);
        }

        diag.Content.AddButton("Wide menu button", OnButtonClicked, style: GameButtonStyle.WideMenu);

        diag.Content.AddButton("Text button", OnButtonClicked, style: GameButtonStyle.Text);

        diag
            .PrintVisualTree()
            .Show(stack);
    }

    void OnButtonClicked()
    {
        diagShower.Create().SetMessage("You clicked a button")
            .SetConfirmButton(() => { }, "OK")
            .Show();
    }

}
