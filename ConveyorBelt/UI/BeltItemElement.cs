namespace ConveyorBelt.UI;

class BeltItemElement : VisualElement
{
    readonly Image imgGood;
    readonly Image imgWarning;

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

    public BeltItemElement(Sprite warningIcon)
    {
        var s = style;
        s.width = s.height = BeltFragment.IconSize;
        s.position = Position.Absolute;
        s.top = BeltFragment.Padding;

        imgGood = this.AddImage();
        s = imgGood.style;
        s.width = s.height = BeltFragment.IconSize;
        s.position = Position.Absolute;

        imgWarning = this.AddImage(warningIcon);
        s = imgWarning.style;
        s.width = s.height = BeltFragment.IconSize;
        s.position = Position.Absolute;
        imgWarning.SetDisplay(false);
    }
}
