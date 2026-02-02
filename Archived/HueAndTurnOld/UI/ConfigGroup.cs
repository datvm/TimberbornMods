namespace HueAndTurn.UI;

public class ConfigGroup : VisualElement
{

    readonly Label lblHeader;
    readonly Button btnExpand, btnCollapse;

    public VisualElement Content { get; }

    public ConfigGroup()
    {
        var header = this.AddRow().AlignItems().SetMarginBottom(10);

        lblHeader = header.AddGameLabel();
        var spacing = header.AddChild().SetMarginLeftAuto();

        btnExpand = header.AddPlusButton(size: UiBuilder.GameButtonSize.Small)
            .AddAction(() => ToggleExpansion(true));
        btnCollapse = header.AddMinusButton(size: UiBuilder.GameButtonSize.Small)
            .AddAction(() => ToggleExpansion(false));

        Content = this.AddChild().SetMarginBottom();

        // Collapse
        ToggleExpansion(false);
    }

    public ConfigGroup SetHeader(string header)
    {
        lblHeader.text = header;
        return this;
    }

    void ToggleExpansion(bool expand)
    {
        Content.SetDisplay(expand);
        btnExpand.SetDisplay(!expand);
        btnCollapse.SetDisplay(expand);
    }

}
