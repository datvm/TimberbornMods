
namespace TimberUiDemo.Services;

public class MenuService(PanelStack stack, MainMenuPanel menu, VisualElementLoader veLoader, DialogBoxShower diagShower, DropdownItemsSetter dropdownSetter) : ILoadableSingleton
{

    static readonly string[] Assets = [];

    void PrintDebugInfo()
    {
        foreach (var a in Assets)
        {
            veLoader.LoadVisualElement(a).PrintVisualTree(true);
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
        var diag = new DialogBoxElement()
            .SetTitle("Dialog title")
            .AddCloseButton();

        var con = diag.Content;
        con.SetMaxHeight(500);

        con.AddLabelHeader("Label: Header");
        con.AddLabel("Label: Default")
            .SetMarginBottom();

        con.AddLabelHeader("Buttons");

        var menuBtns = con.AddChild(name: "MenuButtons")
            .SetAsRow()
            .SetWrap()
            .SetMarginBottom();
        {
            menuBtns.AddMenuButton("Menu button", OnButtonClicked);
            menuBtns.AddMenuButton("Medium", OnButtonClicked, size: GameButtonSize.Medium);
            menuBtns.AddMenuButton("Large", OnButtonClicked, size: GameButtonSize.Large);
        }

        con.AddChild()
            .SetMarginBottom()
            .AddMenuButton("Menu button - Stretched", OnButtonClicked, stretched: true);

        var otherBtns = con.AddChild()
            .SetAsRow()
            .SetMarginBottom();
        {
            otherBtns
                .AddButton("Wide menu button", onClick: OnButtonClicked, style: GameButtonStyle.WideMenu)
                .SetMarginRight();
            otherBtns.AddButton("Text button", onClick: OnButtonClicked, style: GameButtonStyle.Text);
        }

        con.AddToggle(
            "Toggle (Checkbox)",
            onValueChanged: (c) => ShowMsgBox($"You {(c ? "checked" : "unchecked")} the Toggle"));

        // Slider
        var sliderCon = con.AddChild()
            .SetMarginBottom();
        {
            sliderCon
                .AddSliderInt(label: "SliderInt", values: new(0, 100, 50))
                .AddEndLabel(v => $"{v}%")
                .SetMarginBottom();

            sliderCon   
                .AddSlider(label: "Slider (Float)", values: new(-.5f, .5f, 0f))
                .AddEndLabel(v => $"{v:P2}");
        }

        // Dropdown
        var dropdownCon = con.AddChild()
            .SetAsRow()
            .SetMarginBottom();

        dropdownCon.AddLabel("Dropdown:")
            .SetMarginRight();
        var dropdown = dropdownCon.AddMenuDropdown()
            .SetMarginBottom()
            .SetFlexGrow();
        

        // Scroll & ListView
        con.AddLabelHeader("ScrollView");
        {
            var scrollView = con.AddScrollView()
                .SetMaxHeight(100);

            for (int i = 0; i < 20; i++)
            {
                scrollView.AddLabel($"ScrollView's item {i + 1}");
            }
        }

        con.AddLabelHeader("ListView");
        var lst = con.AddListView()
            .SetMaxHeight(200);
        PopulateListView(lst);

        diag
            .PrintVisualTree(true)
            .Show(veLoader, stack);

        dropdown.SetItems(dropdownSetter, [.. Enumerable.Range(0, 10).Select(i => $"Item {i}")], "Item 3");
        dropdown.ValueChanged += (_, _) => ShowMsgBox($"You selected {dropdown.GetSelectedValue()} (index {dropdown.GetSelectedIndex()})");
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
