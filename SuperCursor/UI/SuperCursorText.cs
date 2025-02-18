namespace SuperCursor.UI;

public class SuperCursorText : NineSliceVisualElement, ILoadableSingleton
{
    readonly PanelStack panelStack;
    readonly Label label;

    public bool Visible
    {
        get => style.display == DisplayStyle.Flex;
        set => this.ToggleDisplayStyle(value);
    }
    
    public string Text
    {
        get => label.text;
        set => label.text = value;
    }

    public SuperCursorText(PanelStack panelStack)
    {
        classList.AddRange(["bg-striped--green", "bg-sub-box--frame"]);

        this.panelStack = panelStack;

        label = new NineSliceLabel();
        label.classList.AddRange(["game-text-normal", "text--yellow"]);
        Add(label);

        Visible = false;
        style.position = Position.Absolute;
        style.paddingLeft = style.paddingBottom = style.paddingRight = style.paddingTop = 10;
    }

    public void MoveTo(float x, float y)
    {
        style.left = MathF.Min(x, Screen.width - layout.width);
        style.top = MathF.Min(y, Screen.height - layout.height);

    }

    public void Load()
    {
        panelStack._root.Add(this);
    }

}
