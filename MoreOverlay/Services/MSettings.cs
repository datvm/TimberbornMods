namespace MoreOverlay.Services;

[BindSingleton(Contexts = BindAttributeContext.All)]
public class MSettings(
    IContainer container,
    ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository)
    : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId => nameof(MoreOverlay);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    public ModSetting<bool> SameOnHover { get; } = new(true, Create("SameOnHover"));
    public bool SameOnHoverValue => SameOnHover.Value;

    public ModSetting<bool> Highlight { get; } = new(true, Create("Highlight"));
    public bool HighlightValue => Highlight.Value;

    readonly Dictionary<IMoreOverlayProvider, ModSetting<bool>> overlayToggles = [];
    public bool IsOverlayTypeEnabled(IMoreOverlayProvider provider) => overlayToggles.TryGetValue(provider, out var s) && s.Value;

    public override void OnAfterLoad()
    {
        base.OnAfterLoad();

        AddNonPersistentModSetting(new ReadonlyTextModSetting(Create("EnabledOverlays"), new ReadonlyTextModSetting.TextSettings()));

        var providers = container.GetInstances<IMoreOverlayProvider>();
        foreach (var p in providers)
        {
            var typeName = p.GetType().Name;
            var s = new ModSetting<bool>(true, ModSettingDescriptor.CreateLocalized("LV.MOv.Overlay." + typeName));
            overlayToggles[p] = s;

            AddCustomModSetting(s, "Overlay." + typeName);
        }
    }

    static ModSettingDescriptor Create(string key) => ModSettingDescriptor
        .CreateLocalized("LV.MOv." + key)
        .SetLocalizedTooltip("LV.MOv." + key + "Desc");

}
