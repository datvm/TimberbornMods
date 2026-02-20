namespace BuildingBlueprints.UI;

[BindTransient]
public class BlueprintWorkshopSelectionDialog(
    BlueprintWorkshopServiceResolver resolver,
    PanelStack panelStack,
    VisualElementInitializer veInit,
    ILoc t,
    BuildingBlueprintListingService blueprintListingService
) : DialogBoxElement
{

    readonly IBlueprintWorkshopService ws = resolver.Provider;
    readonly BlueprintWorkshopSelection selection = new();

#nullable disable
    TextField txtExtId;
    Button btnUpload;
#nullable enable

    int selectionCount = 0;
    readonly List<Toggle> chks = [];
    List<SerializableBuildingBlueprint> blueprints = [];

    public void Show()
    {
        SetTitle(t.T("LV.BB.UploadWorkshop"));
        AddCloseButton();

        var parent = Content;

        var btns = parent.AddRow().JustifyContent().SetMarginBottom(5);
        btnUpload = btns.AddMenuButton(t.T("LV.BB.UploadWorkshop"), onClick: OnUIConfirmed);
        btnUpload.enabledSelf = false;

        var titlePanel = parent.AddChild().SetMarginBottom();
        titlePanel.AddLabel(t.T("LV.BB.ExistingId"));
        txtExtId = titlePanel.AddTextField();
        titlePanel.AddLabel(t.T("LV.BB.ExistingIdDesc"));

        var list = parent.AddScrollView().SetHeight(300).SetMarginBottom();
        PopulateList(list);

        Show(veInit, panelStack, confirm: OnConfirm);
    }

    void PopulateList(VisualElement list)
    {
        blueprints = [..blueprintListingService.GetBlueprints()];
        foreach (var bp in blueprints)
        {
            var chk = list.AddToggle(bp.Name, onValueChanged: OnItemCheckChanged).SetMarginBottom(5);
            list.Add(chk);
            chks.Add(chk);
        }
    }

    void OnItemCheckChanged(bool v)
    {
        selectionCount += v ? 1 : -1;
        btnUpload.enabledSelf = selectionCount > 0;
    }

    void OnConfirm()
    {
        selection.ItemId = txtExtId.value;
        
        selection.Blueprints.Clear();
        for (int i = 0; i < chks.Count; i++)
        {
            if (!chks[i].value) { continue; }
            selection.Blueprints.Add(blueprints[i]);
        }

        ws.OpenUploadBlueprint(selection);
    }

}
