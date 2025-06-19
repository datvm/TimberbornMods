namespace MacroManagement.UI;

public class MultiSelectFragment(
    MacroManagementController controller,
    ILoc t,
    InputService inputs,
    InputBindingDescriber hotkeyDescriber
) : IEntityPanelFragment, IInputProcessor
{

#nullable disable
    public EntityPanelFragmentElement Panel { get; private set; }

    Toggle chkPaused, chkRunning;

    Label lblSelect;
    Button btnCopy;
    Button btnSelectDistrict, btnSelectAll;
#nullable enable

    MacroManagementInfo? info;
    bool canSelectDistrict;

    public void ClearFragment()
    {
        inputs.RemoveInputProcessor(this);
        info = null;
        Panel.Visible = false;
    }

    public VisualElement InitializeFragment()
    {
        Panel = new()
        {
            Background = EntityPanelFragmentBackground.PalePurple,
            Visible = false,
        };

        btnCopy = AddButton(Panel, null, RequestCopy);

        AddSelectPanel(Panel);

        return Panel;
    }

    VisualElement AddSelectPanel(VisualElement parent)
    {
        var container = parent.AddChild();

        lblSelect = container.AddLabel("");
        chkRunning = container.AddToggle(t.T("LV.MacM.SelectRunning")).SetMarginBottom(5);
        chkPaused = container.AddToggle(t.T("LV.MacM.SelectPaused")).SetMarginBottom(5);

        btnSelectAll = AddButton(container, "LV.MacM.SelectAll", () => SelectBuildings(MacroManagementSelectionFlags.None));
        btnSelectDistrict = AddButton(container, null, () => SelectBuildings(MacroManagementSelectionFlags.District));

        chkRunning.SetValueWithoutNotify(true);
        chkPaused.SetValueWithoutNotify(true);

        return container;
    }

    NineSliceButton AddButton(VisualElement parent, string? locKey, Action action)
    {
        return parent.AddGameButton(locKey is null ? "" : t.T(locKey), onClick: action)
            .SetFlexGrow()
            .SetPadding(0, 5)
            .SetMarginBottom(5);
    }

    public void ShowFragment(BaseComponent entity)
    {
        if (!controller.TryGetInfo(entity, out info)) { return; }

        UpdateContent();
        Panel.Visible = true;

        inputs.AddInputProcessor(this);
    }

    void UpdateContent()
    {
        var info = this.info!.Value;

        var name = info.LabeledEntity.DisplayName;

        btnCopy.text = hotkeyDescriber.GetCommandWithHotkey(t.T("LV.MacM.BuildAnother", name), MacroManagementController.CopyKeyId);

        lblSelect.text = t.T("LV.MacM.Select", name);

        btnSelectAll.text = hotkeyDescriber.GetCommandWithHotkey(t.T("LV.MacM.SelectAll", name), MacroManagementController.SelectAllKeyId);

        var dc = info.DistrictCenter;
        canSelectDistrict = dc;
        btnSelectDistrict.enabledSelf = canSelectDistrict;
        btnSelectDistrict.text = canSelectDistrict
            ? hotkeyDescriber.GetCommandWithHotkey(
                t.T("LV.MacM.SelectDistrict", controller.GetBadgeName(dc!)),
                MacroManagementController.SelectDistrictKeyId)
            : t.T("LV.MacM.NoDistrict");
    }

    void RequestCopy()
    {
        if (info is null) { return; }

        controller.RequestCopy(info.Value);
    }

    void SelectBuildings(MacroManagementSelectionFlags flags)
    {
        if (info is null) { return; }

        if (chkRunning.value)
        {
            flags |= MacroManagementSelectionFlags.Running;
        }

        if (chkPaused.value)
        {
            flags |= MacroManagementSelectionFlags.Paused;
        }

        controller.RequestSelection(info.Value, flags);
    }

    public void UpdateFragment() { }

    public bool ProcessInput()
    {
        if (inputs.IsKeyDown(MacroManagementController.CopyKeyId))
        {
            RequestCopy();
            return true;
        }
        
        if (inputs.IsKeyDown(MacroManagementController.SelectAllKeyId))
        {
            SelectBuildings(MacroManagementSelectionFlags.None);
            return true;
        }

        if (canSelectDistrict && inputs.IsKeyDown(MacroManagementController.SelectDistrictKeyId))
        {
            SelectBuildings(MacroManagementSelectionFlags.District);
            return true;
        }

        return false;
    }
}
