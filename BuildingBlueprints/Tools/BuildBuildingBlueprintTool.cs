namespace BuildingBlueprints.Tools;

[BindSingleton]
public class BuildBuildingBlueprintTool(
    IContainer container,
    BlueprintPlacementService blueprintPlacementService,
    ToolService toolService,
    ILoc t
) : ITool, IToolDescriptor, ILoadableSingleton
{
#nullable disable
    ToolDescription toolDescription;
    HotkeyEntry flipEntry, rotateClockwiseEntry, rotateCounterClockwiseEntry, ignoreSettingsEntry;
#nullable enable

    public void Load()
    {
        var hotkeySection = container.GetInstance<HotkeyToolDescriptionSection>();

        rotateClockwiseEntry = hotkeySection.AddEntry(BlockObjectPlacementPanel.RotateClockwiseKey);
        rotateCounterClockwiseEntry = hotkeySection.AddEntry(BlockObjectPlacementPanel.RotateCounterclockwiseKey);
        flipEntry = hotkeySection.AddEntry(BlockObjectPlacementPanel.FlipKey);
        ignoreSettingsEntry = hotkeySection.AddEntry(DuplicationInputProcessor.DuplicateSettingsKey);
        hotkeySection.AddEntry(AlternateClickable.AlternateClickableActionKey).Text = t.T("LV.BB.BlueprintBuildToolPartial");

        blueprintPlacementService.OnBlueprintPlacementSettingsChanged += UpdatePlacement;
        UpdatePlacement();

        toolDescription = new ToolDescription.Builder(t.T("LV.BB.BlueprintBuildTool"))
            .AddSection(t.T("LV.BB.BlueprintBuildToolDesc"))
            .AddSection(hotkeySection.Root)
            .Build();
    }

    void UpdatePlacement()
    {
        var orientation = blueprintPlacementService.BlueprintOrientation.ToString()[2..] + "°";

        rotateClockwiseEntry.Text = t.T("LV.BB.BlueprintBuildToolRotateClockwise", orientation);
        rotateCounterClockwiseEntry.Text = t.T("LV.BB.BlueprintBuildToolRotateCounterclockwise", orientation);
        flipEntry.Text = t.T("LV.BB.BlueprintBuildToolFlip", t.TYesNo(blueprintPlacementService.BlueprintFlip));
        ignoreSettingsEntry.Text = t.T("LV.BB.BlueprintBuildToolIgnoreSettings", t.TYesNo(blueprintPlacementService.IgnoreSettings));
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
