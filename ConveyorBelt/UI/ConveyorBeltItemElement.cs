namespace ConveyorBelt.UI;

public class ConveyorBeltItemElement : VisualElement
{

    readonly Image imgGood, imgWarning;

    public Sprite Icon
    {
        get => imgGood.sprite;
        set => imgGood.sprite = value;
    }

    public bool IsStuck
    {
        get => imgWarning.IsDisplayed();
        set => imgWarning.SetDisplay(value);
    }

    public float Progress
    {
        set => style.left = value;
    }

    public ConveyorBeltItemElement(Sprite warningIcon)
    {
        var s = style;
        s.width = s.height = ConveyorBeltFragment.IconSize;
        s.position = Position.Absolute;
        s.top = ConveyorBeltFragment.Padding;

        imgGood = this.AddImage();
        s = imgGood.style;
        s.width = s.height = ConveyorBeltFragment.IconSize;
        s.position = Position.Absolute;

        imgWarning = this.AddImage(warningIcon);
        s = imgWarning.style;
        s.width = s.height = ConveyorBeltFragment.IconSize;
        s.position = Position.Absolute;
    }

}
