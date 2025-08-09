namespace SkyblightWeather.Services;

public class UpdateNotifier : IModUpdateNotifier
{
    public string ModId { get; } = nameof(SkyblightWeather);
    public string Version { get; } = "7.0.4";
    public string MessageLocKey { get; } = "LV.MWSb.Update704";
}
