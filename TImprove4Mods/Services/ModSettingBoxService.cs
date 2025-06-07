namespace TImprove4Mods.Services;

public class ModSettingBoxService(IContainer container) : IPostLoadableSingleton
{

    public void PostLoad()
    {
        var boxType = GetSettingsBoxType();
        if (boxType is null) { return; }

        ChangeModSettingBoxMaxHeight();
    }

    void ChangeModSettingBoxMaxHeight()
    {
        var box = container.GetInstance<ModSettingsBox>();
        box.GetPanel().Q("Box").SetMaxSizePercent(null, 90f);
    }

    Type? GetSettingsBoxType()
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(q => q.GetName().Name == "ModSettings.CoreUI");

        return assembly?.GetTypes()
            .FirstOrDefault(q => q.Name == nameof(ModSettingsBox));
    }

}
