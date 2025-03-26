namespace RealLights;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{
    static readonly ImmutableArray<string> ShadowOptions = [.. Enum.GetValues(typeof(LightShadows)).Cast<LightShadows>().Select(q => q.ToString())];
    
    public event Action SettingsChanged = delegate { };

    public static LightShadows Shadows { get; private set; } = LightShadows.None;

    public override string ModId { get; } = nameof(RealLights);
    public override ModSettingsContext ChangeableOn { get; } = ModSettingsContext.All;

    readonly LimitedStringModSetting shadows = new(
        0,
        [..ShadowOptions.Select(s => new LimitedStringModSettingValue(s, "LV.RL.Shadow" + s))],
        ModSettingDescriptor
            .CreateLocalized("LV.RL.Shadow")
            .SetLocalizedTooltip("LV.RL.ShadowDesc"));

    public override void OnAfterLoad()
    {
        AddCustomModSetting(shadows, nameof(shadows));

        shadows.ValueChanged += (_, _) => UpdateValues();

        UpdateValues();
    }

    void UpdateValues()
    {
        Shadows = Enum.Parse<LightShadows>(shadows.Value);

        SettingsChanged();
    }

    public void Unload()
    {
        UpdateValues();
    }
}
