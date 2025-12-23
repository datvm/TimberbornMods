namespace ModdableWeathers.UI.Settings;

public class WeatherSettingsExportPanel : VisualElement
{
    readonly WatherSettingsExportService exportService;

    public event Action ReloadRequested = null!;

    public WeatherSettingsExportPanel(
        WatherSettingsExportService exportService,
        ILoc t
    )
    {
        this.exportService = exportService;

        var parent = this;

        this.AddLabelHeader(t.T("LV.MW.Profiles"));
        this.AddLabel(t.T("LV.MW.ProfilesDesc"));

        var row = parent.AddRow();
        row.AddMenuButton(t.T("LV.MW.ProfileExport"), onClick: Export).SetFlexGrow();
        row.AddMenuButton(t.T("LV.MW.ProfileImport"), onClick: Import).SetFlexGrow();        
    }

    void Export()
    {
        exportService.RequestExport();
    }

    void Import()
    {
        if (!exportService.RequestImport()) { return; }
        ReloadRequested?.Invoke();
    }

}
