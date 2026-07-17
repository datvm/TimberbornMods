namespace UnityEngine.UIElements;

public static partial class UiBuilderExtensions
{

    extension(VisualElement parent)
    {
        public T AddChild<T>(string? name = default, IEnumerable<string>? classes = default)
            where T : VisualElement, new() 
            => (T)parent.AddChild(typeof(T), name, classes);

        public T AddChild<T>(Func<T> factory) where T : VisualElement
        {
            var el = factory();
            parent.Add(el);
            return el;
        }

        public VisualElement AddChild(Type? type = default, string? name = default, IEnumerable<string>? classes = default)
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

        public VisualElement AddRow(string? name = default)
        {
            var row = parent.AddChild(name: name);
            return row.SetAsRow();
        }

        public VisualElement AddHorizontalContainer(bool marginBottom = true)
        {
            var con = parent.AddChild().SetAsRow();
            if (marginBottom) { con.SetMarginBottom(); }

            return con;
        }

        public Label AddLabel(string? text = default, string? name = default, IEnumerable<string>? additionalClasses = default, GameLabelStyle style = GameLabelStyle.Default)
        {
            var labelClasses = GetClasses(style);

            var label = parent.AddChild<Label>(name, [.. labelClasses, .. additionalClasses ?? []]);
            if (text is not null)
            {
                label.text = text;
            }
            return label;
        }

        public Label AddLabelHeader(string? text = default, string? name = default, IEnumerable<string>? additionalClasses = default)
            => parent.AddLabel(text, name, additionalClasses, GameLabelStyle.Header);

        public Label AddGameLabel(string? text = default, string? name = default, IEnumerable<string>? additionalClasses = default, GameLabelSize size = default, GameLabelColor? color = default, bool bold = default, bool centered = default)
        {
            var labelClasses = GetGameLabelClasses(size, color, bold, centered);
            var label = parent.AddChild<Label>(name, [.. labelClasses, .. additionalClasses ?? []]);
            label.text = text;
            return label;
        }

        T InternalAddScrollView<T>(string? name = default, IEnumerable<string>? additionalClasses = default, bool greenDecorated = true)
            where T : VisualElement, new()
        {
            if (greenDecorated)
            {
                additionalClasses = [.. additionalClasses ?? [], UiCssClasses.ScrollGreenDecorated];
            }
            return parent.AddChild<T>(name, additionalClasses);
        }

        public ScrollView AddScrollView(string? name = default, IEnumerable<string>? additionalClasses = default, bool greenDecorated = true)
            => parent.InternalAddScrollView<ScrollView>(name, additionalClasses, greenDecorated);

        public ScrollView AddGameScrollView(string? name = default, IEnumerable<string>? additionalClasses = default)
            => parent.InternalAddScrollView<ScrollView>(name, [.. additionalClasses ?? [], UiCssClasses.GameScrollView], greenDecorated: false);

        public ListView AddListView(string? name = default, IEnumerable<string>? additionalClasses = default, bool greenDecorated = true) 
            => parent.InternalAddScrollView<ListView>(name, additionalClasses, greenDecorated: greenDecorated);

        public EntityPanelFragmentElement AddFragment(EntityPanelFragmentBackground? background = default, string? name = default, IEnumerable<string>? additionalClasses = default)
        {
            var fragment = parent.AddChild<EntityPanelFragmentElement>(name, additionalClasses);

            if (background is not null)
            {
                fragment.Background = background.Value;
            }

            return fragment;
        }

        [Obsolete($"What you are looking for is {nameof(AddToggle)}")]
        public VisualElement AddCheckbox()
        {
            throw new NotImplementedException($"What you are looking for is {nameof(AddToggle)}");
        }

        public Toggle AddToggle(string? text = default, string? name = default, IEnumerable<string>? additionalClasses = default, Action<bool>? onValueChanged = default, ToggleStyle style = default)
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

        public Toggle AddGamePanelToggle(string? text = default, Action<bool>? onValueChanged = default)
            => parent.AddToggle(text, onValueChanged: onValueChanged, style: ToggleStyle.GamePanel);

        public CollapsiblePanel AddCollapsiblePanel(string? title = null, bool expand = true, string? name = default)
        {
            var panel = parent.AddChild<CollapsiblePanel>(name);

            if (title is not null)
            {
                panel.SetTitle(title);
            }

            if (!expand)
            {
                panel.SetExpand(false);
            }

            return panel;
        }
    }

    extension(VisualElement element)
    {
        public VisualElement InsertSelfAsSibling(VisualElement target, int delta)
        {
            var parent = target.parent;
            var index = parent.IndexOf(target);
            parent.Insert(index + delta, element);
            return element;
        }

        public VisualElement InsertSelfBefore(VisualElement target) => element.InsertSelfAsSibling(target, 0);
        public VisualElement InsertSelfAfter(VisualElement target) => element.InsertSelfAsSibling(target, 1);
    }

    extension<T>(T element) where T : VisualElement
    {
        [Obsolete($"Use the built-in {nameof(VisualElement.RemoveFromHierarchy)} instead.")]
        public T RemoveSelf()
        {
            var parent = element.parent;
            if (parent is null) { return element; }

            parent.Remove(element);
            return element;
        }
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

    public static IEnumerable<string> GetClasses(ToggleStyle style) => style switch
    {
        ToggleStyle.Settings => UiCssClasses.ToggleSettings,
        ToggleStyle.GamePanel => UiCssClasses.ToggleGamePanel,
        _ => throw new NotImplementedException(style.ToString()),
    };

}
