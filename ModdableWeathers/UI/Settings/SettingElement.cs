namespace ModdableWeathers.UI.Settings;

public class SettingElement : VisualElement
{

    public readonly PropertyInfo Property;
    public readonly string Name;
    public readonly object Settings;
    readonly ILoc t;

    public SettingElement(NamedPropertyInfo property, object settings, ILoc t)
    {
        this.t = t;
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
        var row = AddTwoColumns();
        row.AddToggle(onValueChanged: v =>
        {
            Property.SetValue(Settings, v);
        })
            .SetFlexGrow().SetFlexShrink()
            .SetValueWithoutNotify((bool)Property.GetValue(Settings));
    }

    void AddWeatherList(ModdableWeatherModifierSettings s)
    {
        this.AddLabel(t.T("LV.MW.AssociatedWeathers"));
    }

    VisualElement AddTwoColumns()
    {
        var row = this.AddRow().AlignItems();

        row.AddLabel(t.T(Name)).SetMarginRight().SetWidth(175).SetFlexShrink(0).SetWrap();
        this.AddLabel(t.T(Name + "Desc"));

        return row;
    }

}
