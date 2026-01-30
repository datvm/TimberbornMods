namespace ConfigurableFaction.UI;

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class SettingsIOPanel : VisualElement
{
    readonly UserSettingsUIController controller;
    readonly ISystemFileDialogService diag;

    public SettingsIOPanel(
        UserSettingsUIControllerScope controllerScope,
        ISystemFileDialogService diag,
        ILoc t
    )
    {
        this.diag = diag;
        controller = controllerScope.Controller;

        var row = this.AddRow();

        row.AddMenuButton(t.T("LV.CF.Import"), onClick: Import).SetFlexGrow().SetMinSize(0);
        row.AddMenuButton(t.T("LV.CF.Export"), onClick: Export).SetFlexGrow().SetMinSize(0);
    }

    void Import()
    {
        var path = diag.ShowOpenFileDialog(".json");
        if (string.IsNullOrEmpty(path)) { return; }

        controller.Import(path);
    }

    void Export()
    {
        var path = diag.ShowSaveFileDialog(".json");
        if (string.IsNullOrEmpty(path)) { return; }

        if (!path.Contains('.'))
        {
            path += ".json";
        }

        controller.Export(path);
    }

}
