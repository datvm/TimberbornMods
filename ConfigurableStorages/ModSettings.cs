using Timberborn.AssetSystem;
using Timberborn.Stockpiles;

namespace ConfigurableStorages;

public class ModSettings : ModSettingsOwner, IUnloadableSingleton
{
    readonly IAssetLoader assets;

    public ModSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository, IAssetLoader assets)
        : base(settings, modSettingsOwnerRegistry, modRepository)
    {
        this.assets = assets;
    }

    protected override string ModId => nameof(ConfigurableStorages);

    RangeIntModSetting globalMultiplier = null!;

    readonly Dictionary<string, ModSetting<int>> values = [];
    static readonly Dictionary<string, int> defaultValues = [];

    protected override void OnAfterLoad()
    {
        {
            globalMultiplier = new RangeIntModSetting(
                2, -9, 10,
                ModSettingDescriptor
                    .CreateLocalized("CS.GlobalMultiplier")
                    .SetLocalizedTooltip("CS.GlobalMultiplierTooltip")
            );

            AddCustomModSetting(globalMultiplier, "global_multiplier");
        }

        var buildings = GetSpecs().ToList();

        foreach (var b in buildings)
        {
            var name = b.Asset.name;

            if (!defaultValues.ContainsKey(name))
            {
                defaultValues[name] = (int)maxCapField.GetValue(b.Asset);
            }

            var def = defaultValues[name];

            var setting = new ModSetting<int>(
                def,
                ModSettingDescriptor.Create(name)
                    .SetTooltip($"Game default: {def}")
                    .SetEnableCondition(() => globalMultiplier.Value == 0)
            );
            AddCustomModSetting(setting, name);

            values[name] = setting;
        }
    }

    static readonly FieldInfo maxCapField = typeof(StockpileSpec).GetField("_maxCapacity", BindingFlags.NonPublic | BindingFlags.Instance);
    public void Unload()
    {
        var globalValue = globalMultiplier.Value;

        foreach (var b in GetSpecs())
        {
            var asset = b.Asset;
            var name = asset.name;

            var endValue = GetStorageValue(globalValue, name);
            if (endValue > 0)
            {
                maxCapField.SetValue(asset, endValue);
            }
        }
    }

    int GetStorageValue(int globalValue, string name)
    {
        if (globalValue == 0)
        {
            if (values.TryGetValue(name, out var value))
            {
                return value.Value;
            }

            return 0;
        }
        else if (globalValue > 0)
        {
            var def = defaultValues[name];
            return def * globalValue;
        }
        else
        {
            var def = defaultValues[name];
            return Math.Max(1, def + (int)(def * globalValue / 10f));
        }
    }

    IEnumerable<LoadedAsset<StockpileSpec>> GetSpecs() => assets.LoadAll<StockpileSpec>("");

}
