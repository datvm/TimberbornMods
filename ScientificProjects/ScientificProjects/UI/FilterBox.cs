namespace ScientificProjects.UI;

public class FilterBox : VisualElement
{

#nullable disable
    Button btnExpand;
    Button btnCollapse;

    VisualElement filterPanel;

    TextField txtFilter;
    ImmutableArray<Toggle> chkFlags;
#nullable enable

    bool expanding;
    public event Action<ScientificProjectFilter> OnFilterChanged = delegate { };

    public FilterBox Init(ILoc t)
    {
        var title = this.AddChild().SetAsRow().SetMarginBottom();

        title.AddGameLabel(text: "LV.SP.Filter".T(t).Bold());
        title.AddChild().SetFlexGrow();

        btnExpand = title.AddPlusButton().AddAction(() => ToggleBox(true));
        btnCollapse = title.AddMinusButton().AddAction(() => ToggleBox(false));

        filterPanel = CreateFilterPanel(t);

        ToggleBox(false);

        return this;
    }

    VisualElement CreateFilterPanel(ILoc t)
    {
        var p = this.AddChild().SetMarginBottom();

        txtFilter = p.AddChild<NineSliceTextField>(name: "Keyword", ["text-field"])
            .SetFlexGrow()
            .SetMarginBottom();
        txtFilter.RegisterCallback<ChangeEvent<string>>(_ => TriggerFilterChange());

        var checks = p.AddChild().SetAsRow();
        chkFlags = [
            CreateFilterToggle(checks, "LV.SP.FilterUnlocked", t),
            CreateFilterToggle(checks, "LV.SP.FilterLocked", t),
            CreateFilterToggle(checks, "LV.SP.FilterOneTime", t),
            CreateFilterToggle(checks, "LV.SP.FilterDaily", t)
        ];
        
        return p;
    }

    Toggle CreateFilterToggle(VisualElement ve, string key, ILoc t)
    {
        var toggle = ve.AddToggle(key.T(t), onValueChanged: _ => TriggerFilterChange());
        toggle.SetValueWithoutNotify(true);

        return toggle;
    }

    void ToggleBox(bool expand)
    {
        expanding = expand;
        filterPanel.ToggleDisplayStyle(expand);
        btnExpand.ToggleDisplayStyle(!expand);
        btnCollapse.ToggleDisplayStyle(expand);

        TriggerFilterChange();
    }

    void TriggerFilterChange()
    {
        var filter = ScientificProjectFilter.Default;

        if (expanding)
        {
            var flags = ScientificProjectFilterFlags.None;

            for (int i = 0; i < chkFlags.Length; i++)
            {
                if (chkFlags[i].value)
                {
                    flags |= (ScientificProjectFilterFlags)(1 << i);
                }
            }

            filter = new(txtFilter.value, flags);
        }

        OnFilterChanged(filter);
    }

}

public readonly record struct ScientificProjectFilter(string? Keyword, ScientificProjectFilterFlags Flags)
{
    public static readonly ScientificProjectFilter Default = new(null, (ScientificProjectFilterFlags) (-1));
}

[Flags]
public enum ScientificProjectFilterFlags
{
    None = 0,
    Unlocked = 1,
    Locked = 2,
    OneTime = 4,
    Daily = 8
}