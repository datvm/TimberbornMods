namespace TimberUi.CommonUi;
public class CollapsiblePanel : VisualElement
{

    readonly Button btnCollapse, btnExpand;
    public VisualElement Container { get; private set; }
    public Label HeaderLabel { get; private set; }
    public bool Expand { get; private set; }

    public CollapsiblePanel()
    {
        var header = this.AddRow().AlignItems().SetMarginBottom(5);

        HeaderLabel = header.AddGameLabel().SetFlexGrow(1);
        HeaderLabel.RegisterCallback<ClickEvent>(_ => ToggleExpand());

        btnCollapse = header.AddMinusButton().AddAction(() => SetExpand(false));
        btnExpand = header.AddPlusButton().AddAction(() => SetExpand(true));

        Container = this.AddChild().SetMarginBottom();

        SetExpand(true);
    }

    public CollapsiblePanel SetTitle(string title)
    {
        HeaderLabel.text = title;
        return this;
    }

    public void ToggleExpand() => SetExpand(!Expand);

    public void SetExpand(bool expand)
    {
        Expand = expand;
        btnCollapse.SetDisplay(expand);
        btnExpand.SetDisplay(!expand);
        Container.SetDisplay(expand);
    }

}