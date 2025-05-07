namespace TImprove4Mods.Services;

public class ModManagerBoxService(
    ModManagerBox modManagerBox,
    ILoc t,
    ModManagementService mods,
    VisualElementInitializer veInit,
    ModCompWarningService modCompWarning
) : ILoadableSingleton
{

    VisualElement root = null!;

    public void Load()
    {
        root = modManagerBox._root;

        UpdateModManagerBox();
        InsertTopContent();
        InsertRestartButton();
    }

    void UpdateModManagerBox()
    {
        var box = root.Q(className: "mod-manager-box");
        box.style.height = new StyleLength(new Length(90, LengthUnit.Percent));
    }

    void InsertTopContent()
    {
        var originalTopButtons = root.Q("TopButtons");

        var container = root.AddChild()
            .SetMargin(marginY: 10)
            .SetFlexShrink(0);
        container.InsertSelfAfter(originalTopButtons);

        var btns = container.AddRow().SetMarginBottom(10);
        btns.style.justifyContent = Justify.SpaceBetween;

        AddButton(btns, "LV.T4Mods.EnableAll", () => mods.ToggleAll(true));
        AddButton(btns, "LV.T4Mods.DisableAll", () => mods.ToggleAll(false));
        AddButton(btns, "LV.T4Mods.CheckForIssue", async () => await modCompWarning.CheckForIssueAsync());
        AddButton(btns, "LV.T4Mods.LoadProfile", mods.LoadProfile);
        AddButton(btns, "LV.T4Mods.SaveProfile", mods.SaveProfile);

        var filters = container.AddRow();
        AddFilterContent(filters);

        veInit.InitializeVisualElement(container);
    }

    void AddFilterContent(VisualElement parent)
    {
        parent.AddTextField("Filter", mods.Filter)
            .SetFlexGrow(1);

        parent.AddToggle("LV.T4Mods.Enabled".T(t), onValueChanged: e => mods.FilterState(true, e))
            .SetFlexShrink(0)
            .SetValueWithoutNotify(true);
        parent.AddToggle("LV.T4Mods.Disabled".T(t), onValueChanged: e => mods.FilterState(false, e))
            .SetFlexShrink(0)
            .SetValueWithoutNotify(true);
    }

    void InsertRestartButton()
    {
        var container = root.AddRow()
            .SetMargin(marginY: 10)
            .SetFlexShrink(0);
        container.style.justifyContent = Justify.SpaceBetween;
        container.style.alignItems = Align.Center;

        var confirmButton = root.Q("ConfirmButton");
        container.InsertSelfAfter(confirmButton);

        AddButton(container, "LV.T4Mods.RestartGame", mods.RestartGame);
        veInit.InitializeVisualElement(container);

        container.Add(confirmButton);
    }

    void AddButton(VisualElement parent, string key, Action onClicked)
    {
        parent.AddButton(key.T(t), onClick: onClicked, style: UiBuilder.GameButtonStyle.Text);
    }

}
