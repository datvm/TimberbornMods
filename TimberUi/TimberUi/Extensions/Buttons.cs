namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

    public static NineSliceButton AddButton(this VisualElement parent, string? text = default, string? name = default, Action? onClick = default, IEnumerable<string>? additionalClasses = default, GameButtonStyle style = GameButtonStyle.Menu, GameButtonSize? size = default, bool stretched = false)
    {
        EventCallback<ClickEvent>? callback = onClick is null
            ? null
            : (_) => onClick();

        return AddButton(parent, text, callback, name, additionalClasses, style, size, stretched);
    }

    public static NineSliceButton AddButton(this VisualElement parent, string? text = default, EventCallback<ClickEvent>? clickCb = default, string? name = default, IEnumerable<string>? additionalClasses = default, GameButtonStyle style = GameButtonStyle.Menu, GameButtonSize? size = default, bool stretched = false)
    {
        var btnClasses = GetClasses(style, size, stretched);

        var btn = parent.AddChild<NineSliceButton>(name, [.. btnClasses, .. additionalClasses ?? []]);

        if (text is not null)
        {
            btn.text = text;
        }

        if (clickCb is not null)
        {
            btn.RegisterCallback(clickCb);
        }

        return btn;
    }

    public static NineSliceButton AddMenuButton(this VisualElement parent, string? text = default, Action? onClick = default, string? name = default, IEnumerable<string>? additionalClasses = default, GameButtonSize? size = default, bool stretched = false)
    {
        return AddButton(parent, text, name, onClick, additionalClasses, GameButtonStyle.Menu, size, stretched);
    }

    public static NineSliceButton AddGameButton(this VisualElement parent, string? text = default, Action? onClick = default, string? name = default, IEnumerable<string>? additionalClasses = default, bool stretched = false)
    {
        return AddButton(parent, text, name, onClick, additionalClasses, GameButtonStyle.Game, stretched: stretched);
    }

    public static NineSliceButton AddGameButtonPadded(this VisualElement parent, string? text = default, Action? onClick = default, string? name = default, IEnumerable<string>? additionalClasses = default, bool stretched = false, int paddingX = 5, int paddingY = 5)
    {
        return AddGameButton(parent, text, onClick, name, additionalClasses, stretched)
            .SetPadding(paddingX, paddingY);
    }

    public static NineSliceButton AddEntityFragmentButton(this VisualElement parent, string? text = default, Action? onClick = default, string? name = default, IEnumerable<string>? additionalClasses = default, EntityFragmentButtonColor color = default)
    {
        // Text is added as label
        var btn = AddButton(parent, text: null, name, onClick, additionalClasses, GameButtonStyle.EntityFragment);

        btn.AddClass(GetColorClass(color));

        var wrapper = btn.AddChild(name: "Content").SetAsRow();

        if (text is not null)
        {
            wrapper.AddLabel(text: text, style: GameLabelStyle.EntityFragment);
        }

        return btn;
    }

    public static NineSliceButton AddStretchedEntityFragmentButton(this VisualElement parent, string? text = default, Action? onClick = default, string? name = default, IEnumerable<string>? additionalClasses = default, EntityFragmentButtonColor color = default, bool stretched = false)
    {
        return AddButton(parent, text, name, onClick,
            [UiCssClasses.LabelEntityPanelText, GetColorClass(color), .. additionalClasses ?? []],
            GameButtonStyle.EntityFragment);
    }

    static string GetColorClass(EntityFragmentButtonColor color) => UiCssClasses.ButtonEntityFragment + "--" + color switch
    {
        EntityFragmentButtonColor.Green => UiCssClasses.Green,
        EntityFragmentButtonColor.Red => UiCssClasses.Red,
        _ => throw new NotImplementedException($"Unknown color {color}"),
    };

    public static T AddAction<T>(this T btn, Action onClick) where T : Button
    {
        return btn.AddAction((_) => onClick());
    }

    public static T AddAction<T>(this T btn, EventCallback<ClickEvent> clickCb) where T : Button
    {
        btn.RegisterCallback(clickCb);
        return btn;
    }

    public static Button AddCloseButton(this VisualElement parent, string? name = default)
    {
        return parent.AddChild<Button>(name: name, classes: [UiCssClasses.CloseButton]);
    }

    public static NineSliceButton AddSquareButton(this VisualElement parent, string? name = default, GameButtonSize size = default, IEnumerable<string>? additionalClasses = default)
    {
        additionalClasses ??= [];

        switch (size)
        {
            case GameButtonSize.Small:
                additionalClasses = [.. additionalClasses, UiCssClasses.ButtonSquare + UiCssClasses.ButtonPfSmall];
                break;
            case GameButtonSize.Large:
                additionalClasses = [.. additionalClasses, UiCssClasses.ButtonSquare + UiCssClasses.ButtonPfLarge];
                break;
        }

        return parent.AddChild<NineSliceButton>(name: name, classes: [UiCssClasses.ButtonSquare, .. additionalClasses]);
    }

    public static NineSliceButton AddPlusButton(this VisualElement parent, string? name = default, GameButtonSize size = default)
    {
        return AddSquareButton(parent, name, size, additionalClasses: UiCssClasses.ButtonPlusClasses);
    }

    public static NineSliceButton AddMinusButton(this VisualElement parent, string? name = default, GameButtonSize size = default)
    {
        return AddSquareButton(parent, name, size, additionalClasses: [UiCssClasses.ButtonMinus]);
    }

    public static IEnumerable<string> GetClasses(GameButtonStyle style, GameButtonSize? size = default, bool stretched = false)
    {
        List<string> result = [];

        var styleClass = style switch
        {
            GameButtonStyle.Text => null,
            GameButtonStyle.Menu => UiCssClasses.ButtonMenu,
            GameButtonStyle.WideMenu => UiCssClasses.ButtonWideMenu,
            GameButtonStyle.BottomBar => UiCssClasses.ButtonBottomBar,
            GameButtonStyle.DevPanel => UiCssClasses.ButtonDevPanel,
            GameButtonStyle.Game => UiCssClasses.ButtonGame,
            GameButtonStyle.EntityFragment => UiCssClasses.ButtonEntityFragment,
            _ => throw new NotImplementedException(style.ToString()),
        };

        if (styleClass is null)
        {
            result.AddRange(UiCssClasses.ButtonText);
        }
        else
        {
            result.Add(styleClass);
        }

        if (style == GameButtonStyle.Game)
        {
            result.Add(UiCssClasses.LabelGameTextNormal);
        }

        if (size is not null)
        {
            var sizeClass = styleClass + size switch
            {
                GameButtonSize.Medium => UiCssClasses.ButtonPfMedium,
                GameButtonSize.Large => UiCssClasses.ButtonPfLarge,
                _ => throw new NotImplementedException(size.ToString()),
            };
            result.Add(sizeClass);
        }

        if (stretched)
        {
            result.Add(styleClass + UiCssClasses.ButtonPfStretched);
        }

        return result;
    }

    public static ClosableAlertElement AddClosableAlert(this VisualElement parent, AlertPanelRowFactory factory, string iconName, string? text, Action? buttonAction, Action? closeButtonAction, string? name = default)
    {
        var el = new ClosableAlertElement(factory, iconName);

        if (text is not null)
        {
            el.SetText(text);
        }

        if (buttonAction is not null)
        {
            el.SetButtonCallback(buttonAction);
        }

        if (closeButtonAction is not null)
        {
            el.AddCloseCallback(closeButtonAction);
        }

        if (name is not null)
        {
            el.Root.name = name;
        }

        parent.Add(el.Root);
        return el;
    }

}

public enum EntityFragmentButtonColor
{
    Green,
    Red
}