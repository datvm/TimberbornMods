namespace ConfigurableTopBar.UI;

public class EditableGoodPanel : VisualElement
{

    readonly Image icon;
    readonly Label lblName;
    readonly Toggle chk;

    public event EventHandler<bool> CheckedChanged = null!;
    public event EventHandler<bool> MoveRequested = null!;

    public EditableGoodSpec GoodSpec { get; private set; } = null!;

    public EditableGoodPanel()
    {
        var panel = this.SetAsRow().AlignItems();

        chk = panel.AddToggle(onValueChanged: OnCheckedChanged).SetMarginRight(10);
        icon = panel.AddImage().SetSize(30, 30).SetMarginRight(10);
        lblName = panel.AddLabel().SetFlexGrow().SetFlexShrink();

        panel.AddMoveButton(true, Move);
        panel.AddMoveButton(false, Move);
    }

    public EditableGoodPanel Init(EditableGoodSpec good, bool selected)
    {
        GoodSpec = good;
        var spec = good.GoodSpec!;

        icon.sprite = spec.Icon.Asset;
        lblName.text = spec.DisplayName.Value;
        chk.SetValueWithoutNotify(selected);

        return this;
    }

    void OnCheckedChanged(bool v) => CheckedChanged(this, v);
    void Move(bool up) => MoveRequested(this, up);

}
