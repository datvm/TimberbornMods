namespace ConfigurableFaction.UI;

public class GroupPanel : VisualElement
{

    readonly Label lblHeader;
    readonly Button btnCollapse, btnExpand;

    public VisualElement Content { get; }

    public GroupPanel()
    {
        var parent = this;

        var header = parent.AddRow().SetMarginBottom(10);
        lblHeader = header.AddLabelHeader().SetFlexGrow(1);
        btnCollapse = header.AddMinusButton()
            .SetFlexShrink(0)
            .AddAction(() => ToggleCollapse(true));
        btnExpand = header.AddPlusButton()
            .SetFlexShrink(0)
            .AddAction(() => ToggleCollapse(false));

        Content = parent.AddChild();

        ToggleCollapse(false);
    }

    void ToggleCollapse(bool collapse)
    {
        Content.SetDisplay(!collapse);
        btnCollapse.SetDisplay(!collapse);
        btnExpand.SetDisplay(collapse);
    }

    public GroupPanel SetHeader(string text)
    {
        lblHeader.text = text;
        return this;
    }

}
