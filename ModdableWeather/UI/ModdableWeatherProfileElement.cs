namespace ModdableWeather.UI;

public class ModdableWeatherProfileElement : VisualElement
{

    readonly ModSettingsOwnerRegistry registry;
    readonly ISystemFileDialogService fsDiag;
    readonly DialogBoxShower diag;
    readonly ModSettingsBox settingsBox;
    readonly ModRepository modRepository;
    readonly ILoc t;

    public ModdableWeatherProfileElement(
        ILoc t,
        ModSettingsOwnerRegistry registry,
        ISystemFileDialogService fsDiag,
        DialogBoxShower diag,
        ModSettingsBox settingsBox,
        ModRepository modRepository
    )
    {
        this.registry = registry;
        this.fsDiag = fsDiag;
        this.diag = diag;
        this.settingsBox = settingsBox;
        this.modRepository = modRepository;
        this.t = t;

        this.AddLabel(t.T("LV.MW.ProfilesDesc"))
            .SetMarginBottom(10);

        var row = this.AddRow();

        var btnImport = row.AddMenuButton(t.T("LV.MW.ProfileImport"), onClick: Import)
            .SetMinSize(0).SetFlexGrow(1).SetFlexShrink(1);
        btnImport.style.flexBasis = 0;

        var btnExport = row.AddMenuButton(t.T("LV.MW.ProfileExport"), onClick: Export)
            .SetMinSize(0).SetFlexGrow(1).SetFlexShrink(1);
        btnExport.style.flexBasis = 0;
    }

    void Import()
    {
        try
        {
            var filePath = fsDiag.ShowOpenFileDialog(".json");
            if (filePath is null) { return; }

            var content = File.ReadAllText(filePath);
            ImportContent(content);

            settingsBox.CloseAndOpenAgain(nameof(ModdableWeather), modRepository);
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    void ImportContent(string content)
    {
        var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
        if (values is null) { return; }

        var settings = GetOwners();
        foreach (var s in settings)
        {
            if (values.TryGetValue(s.WeatherId, out var value))
            {
                s.Import(value);
            }
        }
    }

    void Export()
    {
        try
        {
            var filePath = fsDiag.ShowSaveFileDialog(".json");
            if (filePath is null) { return; }

            if (!filePath.EndsWith(".json"))
            {
                filePath += ".json";
            }

            var content = GetExportContent();
            File.WriteAllText(filePath, content);
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    string GetExportContent()
    {
        Dictionary<string, string> values = [];

        var settings = GetOwners();
        foreach (var s in settings)
        {
            values.Add(s.WeatherId, s.Export());
        }

        return JsonConvert.SerializeObject(values, Formatting.Indented);
    }

    void ShowError(Exception ex)
    {
        diag.Create()
            .SetMessage(t.T("LV.MW.ProfileError", ex.Message))
            .Show();
    }

    ImmutableArray<IExportableSettings> GetOwners() =>
        [.. registry._modSettingOwners.Values
            .SelectMany(q => q)
            .Where(q => q is IExportableSettings)
            .Cast<IExportableSettings>()];

}
