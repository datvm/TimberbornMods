namespace TimberUiDemo.UI;

public class DemoFragment(VisualElementInitializer initializer, DropdownItemsSetter dropdownSetter, IAssetLoader assets) : IEntityPanelFragment
{
    const int Max = 10;

    EntityPanelFragmentElement panel = null!;
    Label lblCoord = null!;

    BlockObject? curr;
    int value;
    Label lblValue = null!;
    GameSliderInt slider = null!;
    Dropdown cbo = null!;
    IEnumerable<(ProgressBar, Label)> pgbBars = null!;

    public VisualElement InitializeFragment()
    {
        panel = new EntityPanelFragmentElement()
        {
            Background = EntityPanelFragmentBackground.Green
        };

        AddCoord();
        AddSquareButtons();
        AddButtons();
        AddFragmentButtons();
        AddSlider();
        AddToggle();
        AddDropdown();
        AddFormattedText();
        AddProgressBars();

        panel.Initialize(initializer);

        cbo.SetItems(dropdownSetter, [.. Enumerable.Range(0, Max + 1).Select(i => $"Item {i + 1}")]);

        return panel;
    }

    void AddCoord()
    {
        var coord = panel.AddHorizontalContainer();
        coord.AddGameLabel("Coordinates".Bold());
        lblCoord = coord.AddGameLabel("N/A");
    }

    void AddSquareButtons()
    {
        var plus = panel.AddHorizontalContainer();

        plus.AddPlusButton(size: GameButtonSize.Small).AddAction(Add);
        plus.AddMinusButton(size: GameButtonSize.Small).AddAction(Subtract).SetMarginRight();

        plus.AddPlusButton(size: GameButtonSize.Medium).AddAction(Add);
        plus.AddMinusButton(size: GameButtonSize.Medium).AddAction(Subtract).SetMarginRight();

        plus.AddPlusButton(size: GameButtonSize.Large).AddAction(Add);
        plus.AddMinusButton(size: GameButtonSize.Large).AddAction(Subtract).SetMarginRight();

        lblValue = plus.AddGameLabel("0");
        ChangeValue(0);
    }

    void AddButtons()
    {
        var con = panel.AddHorizontalContainer();
        con.AddGameButton("Reset", onClick: Reset).SetMarginRight();
        con.AddGameButton("Reset (stretched)", onClick: Reset).SetFlexGrow(1);
    }

    void AddFragmentButtons()
    {
        var con = panel.AddHorizontalContainer();

        con.AddEntityFragmentButton(text: "Min", onClick: Reset, color: EntityFragmentButtonColor.Red)
            .Q("Content").AddInventoryInputImage().SetMarginRight();
        con.AddEntityFragmentButton(text: "Max", onClick: MaxOut, color: EntityFragmentButtonColor.Green)
            .Q("Content").AddInventoryOutputImage();
    }

    void AddSlider()
    {
        slider = panel.AddSliderInt(label: "Value", values: new(0, Max, 0));
        slider.RegisterChange(value => this.value = value);
    }

    void AddToggle()
    {
        panel.AddToggle("A toggle");
    }

    void AddDropdown()
    {
        cbo = panel.AddDropdown()
            .AddChangeHandler((_, index) => ChangeValueTo(index));
    }

    void AddFormattedText()
    {
        panel.AddLabel($"Some text can be {"green".Color(TimberbornTextColor.Green)}, some {"red".Color(TimberbornTextColor.Red)} or " +
            $"{"solid".Color(TimberbornTextColor.Solid)}. Some are {"bold".Bold()}, some are {"italic".Italic()} or {"all".Bold().Italic().Color("#6495ED")}");
    }

    void AddProgressBars()
    {
        var con = panel.AddHorizontalContainer();

        List<(ProgressBar, Label)> bars = [];

        foreach (ProgressBarColor c in Enum.GetValues(typeof(ProgressBarColor)))
        {
            var bar = con.AddProgressBar().SetFlexGrow(1).SetColor(c);
            var label = bar.AddProgressLabel();

            bars.Add((bar, label));
        }

        pgbBars = bars;
    }

    void Add() => ChangeValue(1);
    void Subtract() => ChangeValue(-1);
    void Reset() => ChangeValueTo(0);
    void MaxOut() => ChangeValueTo(Max);
    void ChangeValue(int delta) => ChangeValueTo(value + delta);
    void ChangeValueTo(int value)
    {
        this.value = value;

        panel.Query<Button>(classes: UiCssClasses.ButtonPlus).ForEach(btn => btn.SetEnabled(value < Max));
        panel.Query<Button>(classes: UiCssClasses.ButtonMinus).ForEach(btn => btn.SetEnabled(value > 0));
    }

    public void ClearFragment()
    {
        panel.Visible = false;
    }

    public void ShowFragment(BaseComponent entity)
    {
        curr = entity.GetComponentFast<BlockObject>();
        panel.Visible = curr is not null;

        UpdateFragment();
    }

    public void UpdateFragment()
    {
        if (curr is null) { return; }

        var coords = curr.Coordinates;
        lblCoord.text = $"({coords.x}, {coords.y}, {coords.z})";

        lblValue.text = value.ToString();
        slider.SetValue(value);
        cbo.SetSelectedItem(value);

        foreach (var (bar, label) in pgbBars)
        {
            var ratio = value / (float)Max;
            bar.SetProgress(ratio, label, $"{ratio * 100:F0}%");
        }
    }
}
