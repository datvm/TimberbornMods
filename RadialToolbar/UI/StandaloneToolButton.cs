namespace RadialToolbar.UI;

public class StandaloneToolButton : Button
{
    public const int Size = 60;

    readonly VisualElement image;

    public StandaloneToolButton(NamedIconProvider namedIconProvider) : this(namedIconProvider, null, null) { }

    public StandaloneToolButton(NamedIconProvider namedIconProvider, VisualElement? original, Sprite? sprite = null)
    {
        this.SetSize(Size).SetPadding(2, 5);

        if (original is not null)
        {
            SetBackground(original, namedIconProvider);
        }
        else
        {
            SetDefaultBackground(namedIconProvider);
        }

        var s = style;
        s.alignSelf = Align.Center;

        image = this.AddChild().SetSize(50);
        image.style.alignSelf = Align.Center;
        
        var frame = this.AddChild().SetSize(Size);
        s = frame.style;
        s.position = Position.Absolute;
        s.left = s.top = 0;
        s.backgroundImage = new(namedIconProvider.GetOrLoad("BottomBar-Frame", "UI/Images/BottomBar/button-frame-01"));

        if (sprite is not null)
        {
            SetSprite(sprite);
        }
    }

    public void SetDefaultBackground(NamedIconProvider namedIconProvider)
    {
        style.backgroundImage = new(namedIconProvider.GetOrLoad("BottomBar-bar-bg", "UI/Images/BottomBar/bar-bg"));
    }

    public void SetBackground(VisualElement original, NamedIconProvider namedIconProvider)
    {
        var bgName = "bar-bg";
        var bg = original.Q(className: "bottom-bar-button") ?? original;
        if (bg.ClassListContains("bottom-bar-button--red"))
        {
            bgName = "button-bg-01";
        }
        else if (bg.ClassListContains("bottom-bar-button--blue"))
        {
            bgName = "button-bg-02";
        }
        else if (bg.ClassListContains("bottom-bar-button--green"))
        {
            bgName = "button-bg-03";
        }

        style.backgroundImage = new(namedIconProvider.GetOrLoad("BottomBar-" + bgName, "UI/Images/BottomBar/" + bgName));
    }

    public void SetSprite(Sprite? sprite)
    {
        image.style.backgroundImage = sprite is null ? new(StyleKeyword.None) : new(sprite);
    }

}
