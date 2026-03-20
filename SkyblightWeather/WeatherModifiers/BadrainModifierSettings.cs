namespace SkyblightWeather.WeatherModifiers;

public class BadrainModifierSettings : ModdableWeatherModifierSettings
{

    [Description("LV.MWSb.BadrainLimitMoisture")]
    public bool LimitMoisture { get; set; }

    [Description("LV.MWSb.BadrainMoistureRange")]
    public int LimitMoistureRange { get; set; }

    [Description("LV.MWSb.BadrainContamination")]
    public bool LandContamination { get; set; }

    [Description("LV.MWSb.BadrainContaminationDuration")]
    public float LandContaminationDuration { get; set; }

    [Description("LV.MWSb.BadrainClearDuration")]
    public float LandClearDuration { get; set; }

    [Description("LV.MWSb.BadrainSickBeavers")]
    public bool SickBeavers { get; set; }

    [Description("LV.MWSb.BadrainSickBeaversMin")]
    public int SickBeaversMin { get; set; }

    [Description("LV.MWSb.BadrainSickBeaversMax")]
    public int SickBeaversMax { get; set; }

    [Description("LV.MWSb.BadrainDamageBots")]
    public bool DamageBots { get; set; }

    [Description("LV.MWSb.BadrainBotDamage")]
    public int BotDamage { get; set; }

    public override void InitializeNewSettings()
    {
        LimitMoisture = true;
        LimitMoistureRange = 3;

        LandContamination = false;
        LandContaminationDuration = .8f;
        LandClearDuration = 12f;

        SickBeavers = false;
        SickBeaversMin = 3;
        SickBeaversMax = 6;

        DamageBots = true;
        BotDamage = 1;
    }

}
