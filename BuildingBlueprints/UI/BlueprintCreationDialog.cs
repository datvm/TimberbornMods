namespace BuildingBlueprints.UI;

[BindTransient]
public class BlueprintCreationDialog(
    VisualElementInitializer veInit,
    PanelStack panelStack,
    ILoc t,
    TemplateNameMapper templateMapper,
    NamedIconProvider namedIconProvider,
    DialogService diag,
    BuildingBlueprintsService buildingBlueprintsService,
    IGoodService goods
) : DialogBoxElement
{

#nullable disable
    BlueprintSelectionInfo selection;
    TextField txtName;
#nullable enable

    public void Show(BlueprintSelectionInfo selection)
    {
        SetDialogPercentSize(height: .75f);

        this.selection = selection;

        SetTitle(t.T("LV.BB.BlueprintCreate"));
        AddCloseButton();

        var parent = Content;

        var nameRow = parent.AddRow().AlignItems();
        txtName = nameRow.AddTextField().SetFlexGrow();
        txtName.SetValueWithoutNotify(selection.Name);
        nameRow.AddMenuButton(t.T("LV.BB.Save"), onClick: OnSaveRequested);

        parent.AddLabel(t.T("LV.BB.BlueprintCreateArea", selection.Area.width, selection.Area.height)).SetMarginBottom();

        AddBuildings(parent.AddChild().SetMarginBottom());
        AddCosts(parent.AddChild().SetMarginBottom());

        Show(veInit, panelStack);
    }

    void AddBuildings(VisualElement parent)
    {
        parent.AddLabelHeader(t.T("LV.BB.BlueprintCreateBuildings", selection.AllBuildingsCount));

        foreach (var (b, count) in selection.BuildingsCount)
        {
            var exist = templateMapper.TryGetTemplate(b, out var template);

            Sprite icon;
            string name;
            if (exist)
            {
                var label = template.GetSpec<LabeledEntitySpec>();
                icon = label.Icon.Asset;
                name = t.T(label.DisplayNameLocKey);
            }
            else
            {
                const string QuestionMarkIcon = "question-mark";

                icon = namedIconProvider.GetOrLoadGameIcon(QuestionMarkIcon, QuestionMarkIcon);
                name = t.T("LV.BB.UnknownBuilding", template);
            }

            parent.AddIconSpan(icon, prefixText: count + "×", postfixText: name, size: 40)
                .SetMarginBottom(5)
                .JustifyContent(Justify.FlexStart);
        }
    }

    void AddCosts(VisualElement parent)
    {
        parent.AddLabelHeader(t.T("LV.BB.BlueprintCreateCosts"));

        foreach (var g in selection.Costs)
        {
            parent.AddIconSpan().SetGood(goods, g.GoodId, g.Amount.ToString(), showName: true)
                .JustifyContent(Justify.FlexStart)
                .SetMarginBottom(5);
        }
    }

    async void OnSaveRequested()
    {
        var name = txtName.value;

        var validation = BuildingBlueprintPersistentService.ValidateNewBlueprintName(name, out _);
        switch (validation)
        {
            case BuildingBlueprintPersistentService.ValidationResult.Valid:
                break;            
            case BuildingBlueprintPersistentService.ValidationResult.AlreadyExists:
                if (!await diag.ConfirmAsync("LV.BB.FileDuplicateWarning", localized: true)) { return; }
                break;
            case BuildingBlueprintPersistentService.ValidationResult.Invalid:
                diag.Alert("LV.BB.InvalidName", true);
                return;
        }

        buildingBlueprintsService.ProcessAndSaveBlueprint(name, selection);
        OnUIConfirmed();
    }

}