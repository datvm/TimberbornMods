namespace BuildingBlueprints.Tools;

[BindSingleton]
public class BuildBuildingBlueprintTool(
    IContainer container,
    BlueprintPlacementService blueprintPlacementService,
    ToolService toolService,
    ILoc t
) : ITool, IToolDescriptor, ILoadableSingleton, IConstructionModeEnabler
{
#nullable disable
    ToolDescription toolDescription;
    HotkeyEntry flipEntry, rotateClockwiseEntry, rotateCounterClockwiseEntry, ignoreSettingsEntry, highPerformanceEntry, nudgeEntry;
    MultiHotkeyEntry nudgeMoveKeys, nudgeUpDownKeys;
#nullable enable

    public void Load()
    {
        var hotkeySection = container.GetInstance<HotkeyToolDescriptionSection>();

        rotateClockwiseEntry = hotkeySection.AddEntry(BlockObjectPlacementPanel.RotateClockwiseKey);
        rotateCounterClockwiseEntry = hotkeySection.AddEntry(BlockObjectPlacementPanel.RotateCounterclockwiseKey);
        flipEntry = hotkeySection.AddEntry(BlockObjectPlacementPanel.FlipKey);
        ignoreSettingsEntry = hotkeySection.AddEntry(DuplicationInputProcessor.DuplicateSettingsKey);
        highPerformanceEntry = hotkeySection.AddEntry(BlueprintPlacementService.HighPerformanceBlueprintKey);
        hotkeySection.AddEntry(AlternateClickable.AlternateClickableActionKey).Text = t.T("LV.BB.BlueprintBuildToolPartial");

        nudgeEntry = hotkeySection.AddEntry(BlueprintPlacementService.NudgeKey);

        nudgeMoveKeys = hotkeySection.AddMultiEntry(BlueprintPlacementService.CameraMoveKeys).SetMargin(left: 10);
        nudgeMoveKeys.Text = t.T("LV.BB.NudgeMove");

        nudgeUpDownKeys = hotkeySection.AddMultiEntry(BlueprintPlacementService.CameraRotationKeys).SetMargin(left: 10);
        nudgeUpDownKeys.Text = t.T("LV.BB.NudgeVerticalMove");

        blueprintPlacementService.OnBlueprintPlacementSettingsChanged += UpdateSettingTexts;
        UpdateSettingTexts();

        toolDescription = new ToolDescription.Builder(t.T("LV.BB.BlueprintBuildTool"))
            .AddSection(t.T("LV.BB.BlueprintBuildToolDesc"))
            .AddSection(hotkeySection.Root)
            .Build();
    }

    void UpdateSettingTexts()
    {
        var orientation = blueprintPlacementService.BlueprintOrientation.ToString()[2..] + "°";

        rotateClockwiseEntry.Text = t.T("LV.BB.BlueprintBuildToolRotateClockwise", orientation);
        rotateCounterClockwiseEntry.Text = t.T("LV.BB.BlueprintBuildToolRotateCounterclockwise", orientation);
        flipEntry.Text = t.T("LV.BB.BlueprintBuildToolFlip", t.TYesNo(blueprintPlacementService.BlueprintFlip));
        ignoreSettingsEntry.Text = t.T("LV.BB.BlueprintBuildToolIgnoreSettings", t.TYesNo(blueprintPlacementService.IgnoreSettings));

        var highPerformanceText = t.T("LV.BB.HighPerformanceModeStatus", t.TYesNo(blueprintPlacementService.HighPerformanceMode));
        if (blueprintPlacementService.HighPerformanceMode)
        {
            highPerformanceText = highPerformanceText.Color(TimberbornTextColor.Red);
        }
        highPerformanceEntry.Text = highPerformanceText;

        var nudging = blueprintPlacementService.IsNudging;
        nudgeEntry.Text = t.T("LV.BB.BlueprintBuildToolNudge", t.TYesNo(nudging));
        nudgeMoveKeys.SetDisplay(nudging);
        nudgeUpDownKeys.SetDisplay(nudging);
    }

    public ToolDescription DescribeTool() => toolDescription;

    public async void Enter()
    {
        var diag = container.GetInstance<BlueprintSelectionDialog>();
        var blueprint = await diag.PickAsync();
        if (blueprint is null)
        {
            container.GetInstance<ToolService>().SwitchToDefaultTool();
            toolService.SwitchToDefaultTool();
            return;
        }

        while (await blueprintPlacementService.PlaceAsync(blueprint) is not null) { }
    }

    public void Exit()
    {
        blueprintPlacementService.Stop();
    }

}
