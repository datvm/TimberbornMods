namespace BuildingBlueprints.UI;

[BindTransient]
public class BlueprintEditDialog(
    VisualElementInitializer veInit,
    PanelStack panelStack,
    ILoc t,
    BuildingBlueprintTagService tagService,
    DialogService diag
) : DialogBoxElement
{
#nullable disable
    TextField txtName;
    string originalName;
    Label lblRenameDuplicate;

    TextField txtTags;

    Button btnDone;
#nullable enable

    bool Renaming => txtName.value != originalName;

    public async Task ShowEditAsync(ParsedBlueprintInfo blueprint)
    {
        SetTitle(t.T("LV.BB.EditBlueprint"));
        AddCloseButton();

        var parent = Content;

        var buttons = parent.AddRow().SetMarginBottom().AlignItems();
        buttons.AddGameButtonPadded(t.T("LV.BB.Delete"), onClick: () => OnDeleteClicked(blueprint.RawInfo)).SetFlexShrink(0).SetMarginRight(10);
        btnDone = buttons.AddMenuButton(t.T("LV.BB.EditBlueprint"), onClick: OnUIConfirmed).SetFlexGrow();

        var renamePanel = parent.AddChild().SetMarginBottom();
        renamePanel.AddLabel(t.T("LV.BB.Rename"));
        txtName = renamePanel.AddTextField(changeCallback: OnNameChanged);
        lblRenameDuplicate = renamePanel.AddLabel(t.T("LV.BB.RenameExist")).SetDisplay(false);

        originalName = BuildingBlueprintListingService.GetNameFromFilePath(blueprint.RawInfo.Source.FilePath);
        txtName.SetValueWithoutNotify(originalName);

        var tagsPanel = parent.AddChild();
        tagsPanel.AddLabel(t.T("LV.BB.Tags"));
        txtTags = tagsPanel.AddTextField().SetMarginBottom(10);
        var currTags = blueprint.RawInfo.Tags;
        txtTags.text = string.Join(", ", currTags);

        var quickAdd = tagsPanel.AddRow().AlignItems().SetWrap();
        quickAdd.AddLabel(t.T("LV.BB.TagQuickAdd")).SetMarginRight(10);
        foreach (var t in tagService.TagNamesWithoutUntagged)
        {
            if (currTags.Contains(t)) { continue; }

            quickAdd.AddGameButtonPadded(t, onClick: () => AddTag(t)).SetMarginRight(10);
        }

        var confirmed = await ShowAsync(veInit, panelStack);
        if (confirmed)
        {
            UpdateBlueprint(blueprint.RawInfo);
        }
    }

    HashSet<string> ParseTags(string? input = null)
    {
        input ??= txtTags.text;

        var parts = input.Split(',');
        HashSet<string> result = [];

        foreach (var p in parts)
        {
            var tag = p.Trim();
            if (!string.IsNullOrEmpty(tag))
            {
                result.Add(tag);
            }
        }

        return result;
    }

    void AddTag(string tag)
    {
        var curr = ParseTags();
        if (curr.Add(tag))
        {
            txtTags.text = string.Join(", ", curr);
        }
    }

    void UpdateBlueprint(SerializableBuildingBlueprint original)
    {
        var newBlueprint = original with
        {
            Tags = ParseTags(),
        };

        var path = original.Source.FilePath;
        BuildingBlueprintPersistentService.DeleteBlueprintFile(path);
        BuildingBlueprintPersistentService.SaveBlueprintToFile(txtName.value, newBlueprint);
    }

    void OnNameChanged(string name)
    {
        var valid = !Renaming
            || BuildingBlueprintPersistentService.ValidateNewBlueprintName(name, out _) == BuildingBlueprintPersistentService.ValidationResult.Valid;

        btnDone.enabledSelf = valid;
        lblRenameDuplicate.SetDisplay(!valid);
    }

    async void OnDeleteClicked(SerializableBuildingBlueprint original)
    {
        if (!await diag.ConfirmAsync(t.T("LV.BB.DeleteConfirm"))) { return; }

        BuildingBlueprintPersistentService.DeleteBlueprintFile(original.Source.FilePath);
        OnUICancelled();
    }

}
