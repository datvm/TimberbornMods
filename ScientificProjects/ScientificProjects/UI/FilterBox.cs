namespace ScientificProjects.UI;

public class FilterBox : CollapsiblePanel
{

    readonly ImmutableArray<Toggle> chkFlags;
    readonly TextField txtFilter;

    public event Action<ScientificProjectFilter> OnFilterChanged = delegate { };

    public FilterBox(ILoc t)
    {
        SetTitle(t.T("LV.SP.Filter"));

        var p = Container;

        txtFilter = p.AddTextField(name: "ScientificProjectKeyword", changeCallback: _ => TriggerFilterChange())
            .SetFlexGrow()
            .SetMarginBottom();

        var checks = p.AddChild().SetAsRow();
        chkFlags = [
            CreateFilterToggle("LV.SP.FilterUnlocked"),
            CreateFilterToggle("LV.SP.FilterLocked"),
            CreateFilterToggle("LV.SP.FilterOneTime"),
            CreateFilterToggle("LV.SP.FilterDaily"),
        ];

        SetExpand(false);
        ExpandChanged += _ => TriggerFilterChange();

        Toggle CreateFilterToggle(string key)
        {
            var toggle = checks.AddToggle(key.T(t), onValueChanged: _ => TriggerFilterChange());
            toggle.SetValueWithoutNotify(true);

            return toggle;
        }
    }

    void TriggerFilterChange()
    {
        var filter = ScientificProjectFilter.Default;

        if (Expand)
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
    public static readonly ScientificProjectFilter Default = new(null, (ScientificProjectFilterFlags)(-1));
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