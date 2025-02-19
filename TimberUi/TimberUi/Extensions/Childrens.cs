namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

    public static T AddChild<T>(this VisualElement parent, string? name = default, IEnumerable<string>? classes = default)
        where T : VisualElement, new()
    {
        return (T)parent.AddChild(typeof(T), name, classes);
    }

    public static VisualElement AddChild(this VisualElement parent, Type? type = default, string? name = default, IEnumerable<string>? classes = default)
    {
        type ??= typeof(VisualElement);

        var child = Activator.CreateInstance(type) as VisualElement
            ?? throw new InvalidOperationException("Failed to create VisualElement instance.");

        parent.Add(child);

        if (name is not null)
        {
            child.name = name;
        }

        if (classes is not null)
        {
            child.classList.AddRange(classes);
        }

        return child;
    }

    public static VisualElement InsertSelfAsSibling(this VisualElement element, VisualElement target, int delta)
    {
        var parent = target.parent;
        var index = parent.IndexOf(target);
        parent.Insert(index + delta, element);
        return element;
    }

    public static VisualElement InsertSelfBefore(this VisualElement element, VisualElement target) => element.InsertSelfAsSibling(target, 0);
    public static VisualElement InsertSelfAfter(this VisualElement element, VisualElement target) => element.InsertSelfAsSibling(target, 1);

    public static NineSliceButton AddButton(this VisualElement parent, string text, string? name = default, Action? onClick = default, IEnumerable<string>? additionalClasses = default, GameButtonStyle style = GameButtonStyle.Menu, GameButtonSize? size = default, bool stretched = false)
    {
        EventCallback<ClickEvent>? callback = onClick is null
            ? null
            : (_) => onClick();

        return AddButton(parent, text, callback, name, additionalClasses, style, size, stretched);
    }

    public static NineSliceButton AddButton(this VisualElement parent, string text, EventCallback<ClickEvent>? clickCb, string? name = default, IEnumerable<string>? additionalClasses = default, GameButtonStyle style = GameButtonStyle.Menu, GameButtonSize? size = default, bool stretched = false)
    {
        var btnClasses = GetClasses(style, size, stretched);

        var btn = parent.AddChild<NineSliceButton>(name, [.. btnClasses, .. (additionalClasses ?? [])]);
        btn.text = text;

        if (clickCb is not null)
        {
            btn.RegisterCallback(clickCb);
        }

        return btn;
    }

    public static NineSliceButton AddMenuButton(this VisualElement parent, string text, Action? onClick = default, string? name = default, IEnumerable<string>? additionalClasses = default, GameButtonSize? size = default, bool stretched = false)
    {
        return AddButton(parent, text, name, onClick, additionalClasses, GameButtonStyle.Menu, size, stretched);
    }

    public static Label AddLabel(this VisualElement parent, string text, string? name = default, IEnumerable<string>? additionalClasses = default, GameLabelStyle style = GameLabelStyle.Default)
    {
        var labelClasses = GetClasses(style);

        var label = parent.AddChild<Label>(name, [.. labelClasses, .. (additionalClasses ?? [])]);
        label.text = text;
        return label;
    }

    public static Label AddLabelHeader(this VisualElement parent, string text, string? name = default, IEnumerable<string>? additionalClasses = default)
        => parent.AddLabel(text, name, additionalClasses, GameLabelStyle.Header);

    public static Label AddLabelGame(this VisualElement parent, string text, string? name = default, IEnumerable<string>? additionalClasses = default, GameLabelSize size = default, GameLabelColor? color = default, bool bold = default, bool centered = default)
    {
        var labelClasses = GetGameLabelClasses(size, color, bold, centered);
        var label = parent.AddChild<Label>(name, [.. labelClasses, .. (additionalClasses ?? [])]);
        label.text = text;
        return label;
    }

    public static T AddWrappedChild<TWrapper, T>(this VisualElement parent, string? name = default, IEnumerable<string>? additionalClasses = default, IEnumerable<string>? additionalWrapperClasses = default) 
        where T: VisualElement, new()
        where TWrapper : VisualElement, new()
    {
        var wrapper = parent.AddChild<TWrapper>(classes: additionalWrapperClasses);
        return wrapper.AddChild<T>(name, additionalClasses);
    }

    static T InternalAddScrollView<T>(this VisualElement parent, string? name = default, IEnumerable<string>? additionalClasses = default, IEnumerable<string>? additionalWrapperClasses = default, bool greenDecorated = true)
        where T : VisualElement, new()
    {
        var wrapper = parent.AddChild(classes: [UiCssClasses.ScrollViewClass, .. (additionalWrapperClasses ?? [])]);
        if (greenDecorated)
        {
            wrapper.classList.Add(UiCssClasses.ScrollGreenDecorated);
        }
        return wrapper.AddChild<T>(name, additionalClasses);
    }

    public static ScrollView AddScrollView(this VisualElement parent, string? name = default, IEnumerable<string>? additionalClasses = default, IEnumerable<string>? additionalWrapperClasses = default, bool greenDecorated = true)
    {
        return InternalAddScrollView<ScrollView>(parent, name, additionalClasses, additionalWrapperClasses, greenDecorated);
    }

    public static ListView AddListView(this VisualElement parent, string? name = default, IEnumerable<string>? additionalClasses = default, bool greenDecorated = true)
    {
        return InternalAddScrollView<ListView>(parent, name, additionalClasses, greenDecorated: greenDecorated);
    }

    public static EntityPanelFragmentElement AddFragment(this VisualElement parent, EntityPanelFragmentBackground? background = default, string? name = default, IEnumerable<string>? additionalClasses = default)
    {
        var fragment = parent.AddChild<EntityPanelFragmentElement>(name, additionalClasses);

        if (background is not null)
        {
            fragment.Background = background.Value;
        }

        return fragment;
    }

    [Obsolete($"What you are looking for is {nameof(AddToggle)}")]
    public static VisualElement AddCheckbox(this VisualElement parent)
    {
        throw new NotImplementedException($"What you are looking for is {nameof(AddToggle)}");
    }

    public static Toggle AddToggle(this VisualElement parent, string? text = default, string? name = default, IEnumerable<string>? additionalClasses = default, Action<bool>? onValueChanged = default, ToggleStyle style = default)
    {
        var classes = GetClasses(style);

        var toggle = parent.AddChild<Toggle>(name, [.. classes, .. (additionalClasses ?? [])]);
        toggle.text = text;

        if (onValueChanged is not null)
        {
            toggle.RegisterValueChangedCallback((e) => onValueChanged(e.newValue));
        }

        return toggle;
    }

    public static IEnumerable<string> GetClasses(GameLabelStyle style, GameLabelSize size = default, GameLabelColor? color = default, bool bold = default) => style switch
    {
        GameLabelStyle.Default => ["text--default"],
        GameLabelStyle.Header => ["text--header"],
        GameLabelStyle.Game => GetGameLabelClasses(size, color, bold),
        _ => [],
    };

    public static IEnumerable<string> GetGameLabelClasses(GameLabelSize size = default, GameLabelColor? color = default, bool bold = default, bool centered = default)
    {
        List<string> result = [
            size switch
            {
                GameLabelSize.Normal =>  UiCssClasses.LabelGameTextNormal,
                GameLabelSize.Big => UiCssClasses.LabelGameTextBig,
                _ => throw new NotImplementedException(size.ToString()),
            },
        ];

        if (color is not null && color.Value != GameLabelColor.Default)
        {
            result.Add(UiCssClasses.LabelGamePrefix + color.Value switch
            {
                GameLabelColor.Yellow => UiCssClasses.Yellow,
                _ => throw new NotImplementedException(color.ToString()),
            });
        }

        if (bold)
        {
            result.Add(UiCssClasses.LabelGameTextBold);
        }

        if (centered)
        {
            result.Add(UiCssClasses.LabelGameTextCentered);
        }

        return result;
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

    public static IEnumerable<string> GetClasses(ToggleStyle style)
    {
        return style switch
        {
            ToggleStyle.Settings => UiCssClasses.ToggleSettings,
            _ => throw new NotImplementedException(style.ToString()),
        };
    }

}
