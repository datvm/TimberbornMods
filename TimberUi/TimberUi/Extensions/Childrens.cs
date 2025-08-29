namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

    public static T AddChild<T>(this VisualElement parent, string? name = default, IEnumerable<string>? classes = default)
        where T : VisualElement, new()
    {
        return (T)parent.AddChild(typeof(T), name, classes);
    }

    public static T AddChild<T>(this VisualElement parent, Func<T> factory)
        where T : VisualElement
    {
        var el = factory();
        parent.Add(el);
        return el;
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

    public static VisualElement AddRow(this VisualElement parent, string? name = default)
    {
        var row = parent.AddChild(name: name);
        return row.SetAsRow();
    }

    public static VisualElement AddHorizontalContainer(this VisualElement parent, bool marginBottom = true)
    {
        var con = parent.AddChild().SetAsRow();
        if (marginBottom) { con.SetMarginBottom(); }

        return con;
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

    public static T RemoveSelf<T>(this T element) where T : VisualElement
    {
        var parent = element.parent;
        if (parent is null) { return element; }

        parent.Remove(element);
        return element;
    }

    public static Label AddLabel(this VisualElement parent, string? text = default, string? name = default, IEnumerable<string>? additionalClasses = default, GameLabelStyle style = GameLabelStyle.Default)
    {
        var labelClasses = GetClasses(style);

        var label = parent.AddChild<Label>(name, [.. labelClasses, .. additionalClasses ?? []]);
        if (text is not null)
        {
            label.text = text;
        }
        return label;
    }

    public static Label AddLabelHeader(this VisualElement parent, string? text = default, string? name = default, IEnumerable<string>? additionalClasses = default)
        => parent.AddLabel(text, name, additionalClasses, GameLabelStyle.Header);

    public static Label AddGameLabel(this VisualElement parent, string? text = default, string? name = default, IEnumerable<string>? additionalClasses = default, GameLabelSize size = default, GameLabelColor? color = default, bool bold = default, bool centered = default)
    {
        var labelClasses = GetGameLabelClasses(size, color, bold, centered);
        var label = parent.AddChild<Label>(name, [.. labelClasses, .. additionalClasses ?? []]);
        label.text = text;
        return label;
    }

    static T InternalAddScrollView<T>(this VisualElement parent, string? name = default, IEnumerable<string>? additionalClasses = default, bool greenDecorated = true)
        where T : VisualElement, new()
    {
        if (greenDecorated)
        {
            additionalClasses = [.. additionalClasses ?? [], UiCssClasses.ScrollGreenDecorated];
        }
        return parent.AddChild<T>(name, additionalClasses);
    }

    public static ScrollView AddScrollView(this VisualElement parent, string? name = default, IEnumerable<string>? additionalClasses = default, bool greenDecorated = true)
        => InternalAddScrollView<ScrollView>(parent, name, additionalClasses, greenDecorated);

    public static ScrollView AddGameScrollView(this VisualElement parent, string? name = default, IEnumerable<string>? additionalClasses = default)
        => InternalAddScrollView<ScrollView>(parent, name, [.. additionalClasses ?? [], UiCssClasses.GameScrollView], greenDecorated: false);

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

        var toggle = parent.AddChild<Toggle>(name, [.. classes, .. additionalClasses ?? []]);
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
        GameLabelStyle.EntityFragment => [UiCssClasses.LabelEntityPanelText],
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

    public static IEnumerable<string> GetClasses(ToggleStyle style)
    {
        return style switch
        {
            ToggleStyle.Settings => UiCssClasses.ToggleSettings,
            _ => throw new NotImplementedException(style.ToString()),
        };
    }

}
