namespace PowerCopy.Services;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class PowerCopyService(
    BuildingSettingsResolver buildingSettingsResolver
)
{

    public void Copy(BaseComponent source, IEnumerable<BaseComponent> targets, HashSet<IBuildingSettings> settings)
    {
        if (settings.Count == 0) { return; }

        var filter = settings;

        var srcSettings = buildingSettingsResolver.Get(source, filter);
        
        var serialized = srcSettings.ToDictionary(
            s => s.Settings,
            s => s.Settings.Serialize(s.Duplicable));
        filter = [.. serialized.Keys];

        foreach (var dst in targets)
        {
            var dstSettings = buildingSettingsResolver.Get(dst, filter);

            foreach (var (dstComp, dstSetting) in dstSettings)
            {
                if (!dstSetting.CanDeserialize(dstComp)) { continue; }

                dstSetting.Deserialize(serialized[dstSetting], dstComp);
            }
        }
    }

}