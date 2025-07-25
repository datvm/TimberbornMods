namespace PopControl.UI;

public class PopControlDialog : DialogBoxElement
{
    private readonly PanelStack panelStack;
    private readonly PopControlRegistry registry;
    private readonly DropdownItemsSetter dropdownItemsSetter;
    private readonly DistrictCenterRegistry districtCenterRegistry;

    ReadOnlyList<DistrictCenter> districts = [];
    DistrictPopulationControl data = null!;

    readonly string globalText;
    readonly Dropdown cboScope;
    readonly Toggle chkLimitBeavers, chkLimitBots;
    readonly IntegerField txtLimitBeavers, txtLimitBots;

    public PopControlDialog(
        PanelStack panelStack,
        VisualElementInitializer veInit,
        ILoc t,
        PopControlRegistry registry,
        DropdownItemsSetter dropdownItemsSetter,
        DistrictCenterRegistry districtCenterRegistry
    )
    {
        this.panelStack = panelStack;
        this.registry = registry;
        this.dropdownItemsSetter = dropdownItemsSetter;
        this.districtCenterRegistry = districtCenterRegistry;

        globalText = t.T("LV.Pop.Global");

        SetTitle(t.T("LV.Pop.PopControl"));
        AddCloseButton();

        var parent = Content;
        parent.AddGameLabel(t.T("LV.Pop.Scope"));
        cboScope = parent.AddDropdown()
            .AddChangeHandler((_, i) => OnScopeChanged(i))
            .SetMarginBottom();

        var beavers = parent.AddChild().SetMarginBottom();
        (chkLimitBeavers, txtLimitBeavers) = AddLimitPanel("LV.Pop.LimitBeavers", true);
        (chkLimitBots, txtLimitBots) = AddLimitPanel("LV.Pop.LimitBots", false);

        parent.Initialize(veInit);

        (Toggle, NineSliceIntegerField) AddLimitPanel(string key, bool isBeaver)
        {
            var pop = parent.AddChild().SetMarginBottom();
            var chk = pop.AddToggle(t.T(key), onValueChanged: v => OnLimitToggled(v, isBeaver));
            var txt = pop.AddIntField(changeCallback: v => OnLimitChanged(v, isBeaver));

            return (chk, txt);
        }
    }

    void RefreshContent()
    {
        districts = districtCenterRegistry.FinishedDistrictCenters;

        cboScope.SetItems(dropdownItemsSetter, GetScopes());
        cboScope.SetSelectedItem(0);
        OnScopeChanged(0);
    }

    IReadOnlyList<string> GetScopes() => [.. districts.Select(q => q.DistrictName).Prepend(globalText)];

    public async Task ShowAsync()
    {
        RefreshContent();
        await ShowAsync(null, panelStack);
    }

    void OnScopeChanged(int i)
    {
        var district = i == 0 ? null : districts[i - 1];
        data = district ? registry.GetControlFor(district) : registry.Global;

        chkLimitBeavers.SetValueWithoutNotify(data.LimitBeavers);
        chkLimitBots.SetValueWithoutNotify(data.LimitBots);
        txtLimitBeavers.SetValueWithoutNotify(data.Beavers);
        txtLimitBots.SetValueWithoutNotify(data.Bots);
        SetUiEnabled();
    }

    void OnLimitToggled(bool limit, bool isBeaver)
    {
        if (isBeaver)
        {
            data.LimitBeavers = limit;
        }
        else
        {
            data.LimitBots = limit;
        }
        SetUiEnabled();
    }

    void SetUiEnabled()
    {
        txtLimitBeavers.enabledSelf = data.LimitBeavers;
        txtLimitBots.enabledSelf = data.LimitBots;
    }

    void OnLimitChanged(int limit, bool isBeaver)
    {
        if (limit < 0)
        {
            limit = 0;
        }

        if (isBeaver)
        {
            data.Beavers = limit;
        }
        else
        {
            data.Bots = limit;
        }
    }

}
