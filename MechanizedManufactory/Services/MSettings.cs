namespace MechanizedManufactory.Services;

[BindSingleton(Contexts = BindAttributeContext.MainMenu | BindAttributeContext.Game)]
public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    static readonly Color DefaultDoorColor = new(0x75 / 255f, 0xFF / 255f, 0xA5 / 255f);

    public override string ModId => nameof(MechanizedManufactory);

    public ModSetting<int> PowerPerWorker { get; } = new(75, ModSettingDescriptor
        .CreateLocalized("LV.MM.PowerPerWorker")
        .SetLocalizedTooltip("LV.MM.PowerPerWorkerDesc"));

    public ColorModSetting DoorColor { get; } = new(DefaultDoorColor, ModSettingDescriptor
        .CreateLocalized("LV.MM.DoorColor")
        .SetLocalizedTooltip("LV.MM.DoorColorDesc"), false);

    public static int PowerPerWorkerValue { get; private set; }

    public override void OnAfterLoad()
    {
        base.OnAfterLoad();

        PowerPerWorkerValue = PowerPerWorker.Value;
    }

}
