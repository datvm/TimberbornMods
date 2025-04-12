namespace ConfigurableGameSpeed;

public class MSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    ILoc t
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    const string NormalSpeedKey = "NormalSpeedKey";
    const int NormalSpeedCount = 3;
    const int DevSpeedCount = 2;
    const int TotalSpeedCount = NormalSpeedCount + DevSpeedCount;

    public static ImmutableArray<float> DefaultSpeeds = [.25f, 1f, 3f, 7f, 30f, 99f];
    public static float[] Speeds = [..DefaultSpeeds];

    public override string ModId { get; } = nameof(ConfigurableGameSpeed);
    public override ModSettingsContext ChangeableOn { get; } = ModSettingsContext.All;

    public override void OnAfterLoad()
    {
        for (int i = 1; i <= TotalSpeedCount; i++)
        {
            var z = i;

            ModSetting<float> speed = new(DefaultSpeeds[i], ModSettingDescriptor
                .Create(t.T("LV.CGS.Speed", i)));

            if (i > NormalSpeedCount)
            {
                speed.Descriptor.SetLocalizedTooltip("LV.CGS.SpeedDev");
            }

            AddCustomModSetting(speed, NormalSpeedKey + i);

            speed.ValueChanged += (_, v) => Speeds[z] = v;
            Speeds[i] = speed.Value;
        }

        ModSetting<float> slowSpeed = new(DefaultSpeeds[0], ModSettingDescriptor
            .CreateLocalized("LV.CGS.SpeedSlow")
            .SetLocalizedTooltip("LV.CGS.SpeedSlowDesc"));
        slowSpeed.ValueChanged += (_, v) => Speeds[0] = v;
        AddCustomModSetting(slowSpeed, "SlowSpeedKey");
        Speeds[0] = slowSpeed.Value;
    }

    
}
