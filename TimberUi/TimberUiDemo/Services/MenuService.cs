namespace TimberUiDemo.Services;

public class MenuService(PanelStack stack, MainMenuPanel menu, VisualElementLoader veLoader, DialogBoxShower diagShower) : ILoadableSingleton
{

    static readonly string[] Assets = ["Options/SettingsBox", "Options/KeyBindingsBox"];

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
        btnExit.AddMenuButton("Show demo dialog", ShowDemoDialog, name: "ShowDemoDialog", stretched: true)
            .InsertSelfAfter(btnExit);
    }

    void ShowDemoDialog()
    {
        var diag = new DialogBoxElement(true)
            .SetTitle("Dialog title")
            .AddCloseButton();
        var con = diag.Content;

        con.AddLabelHeader("Label: Header");
        con.AddLabel("Label: Default")
            .SetMargin(bottom: 10);

        con.AddLabelHeader("Buttons");

        var menuBtns = con.AddChild(name: "MenuButtons")
            .SetAsRow()
            .SetWrap()
            .SetMargin(bottom: 20);
        {
            menuBtns.AddMenuButton("Menu button", OnButtonClicked);
            menuBtns.AddMenuButton("Medium", OnButtonClicked, size: GameButtonSize.Medium);
            menuBtns.AddMenuButton("Large", OnButtonClicked, size: GameButtonSize.Large);
        }

        con.AddChild()
            .SetMargin(bottom: 20)
            .AddMenuButton("Menu button - Stretched", OnButtonClicked, stretched: true);

        var otherBtns = con.AddChild()
            .SetAsRow()
            .SetMarginBottom();
        {
            otherBtns
                .AddButton("Wide menu button", onClick: OnButtonClicked, style: GameButtonStyle.WideMenu)
                .SetMargin(right: 20);
            otherBtns.AddButton("Text button", onClick: OnButtonClicked, style: GameButtonStyle.Text);
        }

        con.AddToggle("Toggle (Checkbox)", onValueChanged: (c) => ShowMsgBox($"You {(c ? "checked" : "unchecked")} the Toggle"));

        con.AddLabelHeader("ListView (ScrollView+)");
        var lst = con.AddListView()
            .SetMaxHeight(200);
        PopulateListView(lst);

        diag
            .PrintVisualTree()
            .Show(stack);
    }

    void PopulateListView(ListView l)
    {
        l.fixedItemHeight = 50;

        l.makeItem = () =>
        {
            var ve = new VisualElement().SetAsRow();

            ve.AddLabel("Label", "Label").SetMargin(right: 20);
            ve.AddButton("Button", name: "Button", clickCb: OnButtonClicked, style: GameButtonStyle.WideMenu);

            return ve;
        };

        l.bindItem = (el, i) =>
        {
            el.Q<Label>("Label").text = $"Item {i}";
            el.Q<Button>("Button").dataSource = i;
        };

        l.itemsSource = Enumerable.Range(0, 1000).ToList();
    }

    void OnButtonClicked()
    {
        diagShower.Create().SetMessage("You clicked a button")
            .SetConfirmButton(() => { }, "OK")
            .Show();
    }

    void OnButtonClicked(ClickEvent e)
    {
        var index = (e.target as Button)?.dataSource
            ?? throw new InvalidOperationException("Button has no data source");

        ShowMsgBox($"You clicked a button at index {index}");
    }

    void ShowMsgBox(string text)
    {
        diagShower.Create().SetMessage(text)
            .SetConfirmButton(() => { }, "OK")
            .Show();
    }

}
