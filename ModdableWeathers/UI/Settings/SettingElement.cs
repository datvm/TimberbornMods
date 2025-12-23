namespace ModdableWeathers.UI.Settings;

public class SettingElement(
    ILoc t,
    IContainer container
) : VisualElement
{

#nullable disable
    public PropertyInfo Property { get; private set; }
    public string Name { get; private set; }
    public object Settings { get; private set; }
#nullable enable

    public bool IsEnabledProperty { get; private set; }
    public event Action<bool>? OnEnabledChanged;

    public void Init(NamedPropertyInfo property, object settings)
    {
        Settings = settings;
        var prop = Property = property.Property;
        Name = property.Name;

        this.SetMarginBottom(10);

        var type = prop.PropertyType;

        if (type == typeof(int))
        {
            AddIntField();
        }
        else if (type == typeof(float))
        {
            AddFloatField();
        }
        else if (type == typeof(bool))
        {
            AddBoolField();
        }
        else if (
            prop.Name == nameof(ModdableWeatherModifierSettings.Weathers)
            && Settings is ModdableWeatherModifierSettings s)
        {
            AddWeatherList(s);
        }
        else
        {
            throw new NotSupportedException(
                $"Unsupported property type: {prop.PropertyType} for property {prop.Name} of {prop.DeclaringType.FullName}. " +
                $"Please ask the author to add support for it.");
        }
    }

    void AddIntField()
    {
        var row = AddTwoColumns();
        row.AddIntField(changeCallback: v =>
        {
            Property.SetValue(Settings, v);
        })
            .SetFlexGrow().SetFlexShrink()
            .SetValueWithoutNotify((int)Property.GetValue(Settings));
    }

    void AddFloatField()
    {
        var row = AddTwoColumns();
        row.AddFloatField(changeCallback: v =>
        {
            Property.SetValue(Settings, v);
        })
            .SetFlexGrow().SetFlexShrink()
            .SetValueWithoutNotify((float)Property.GetValue(Settings));
    }

    void AddBoolField()
    {
        IsEnabledProperty = Property.IsEnabledProperty();

        var row = AddTwoColumns();
        row.AddToggle(onValueChanged: v =>
        {
            Property.SetValue(Settings, v);

            if (IsEnabledProperty)
            {
                OnEnabledChanged?.Invoke(v);
            }
        })
            .SetFlexGrow().SetFlexShrink()
            .SetValueWithoutNotify((bool)Property.GetValue(Settings));
    }

    void AddWeatherList(ModdableWeatherModifierSettings s)
    {
        var panel = this.AddChild();

        panel.AddLabel(t.T("LV.MW.AssociatedWeathers"));

        foreach (var w in s.Weathers)
        {
            var el = container.GetInstance<WeatherModifierAssociationPanel>();
            el.Init(w.Key, w.Value);

            panel.Add(el);
        }
    }

    VisualElement AddTwoColumns()
    {
        var row = this.AddRow().AlignItems();

        row.AddLabel(t.T(Name)).SetMarginRight().SetWidth(175).SetFlexShrink(0).SetWrap();
        this.AddLabel(t.T(Name + "Desc"));

        return row;
    }

}
