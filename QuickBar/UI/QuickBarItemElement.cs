namespace QuickBar.UI;

public class QuickBarItemElement : VisualElement
{
    static readonly Color HotKeyBgColor = new(1f, 1f, 1f, .5f);

    public IQuickBarItem? Item { get; private set; } = null!;

    public QuickBarItemElement SetItem(IQuickBarItem? item, string? shortcutText, VisualElement ve)
    {
        Item = item;

        Clear();
        Add(ve);

        if (item is not null)
        {
            var btn = ve.Q<Button>("ToolButton");
            btn.classList.Remove("bottom-bar-button--red");
            btn.AddClass("bottom-bar-button--blue");

            btn.AddAction(() => Item!.Activate());

            var icon = ve.Q("ToolImage");
            if (item.Sprite is not null)
            {
                icon.style.backgroundImage = new StyleBackground(item.Sprite);
            }
            else if (item.Texture is not null)
            {
                icon.style.backgroundImage = item.Texture;
            }
            else
            {
                icon.SetDisplay(false);
            }

            if (item is BlockObjectToolQuickBarItem boTool && boTool.Tool.IsLocked())
            {
                ve.Q<Image>("LockIcon").SetDisplay(true);
            }
        }

        if (shortcutText is not null)
        {
            var lbl = ve.Q<TextElement>("BottomText");
            lbl.text = shortcutText;

            var s = lbl.style;
            s.color = Color.black;
            s.backgroundColor = HotKeyBgColor;
            s.width = new StyleLength(StyleKeyword.Auto);
            s.right = 0;
        }

        return this;
    }

}
