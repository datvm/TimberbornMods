namespace ModdableWeathers.UI.Settings;

public class WeatherSettingsDialogFilter
{
    public string Query { get; set; } = "";
    public bool Benign { get; set; } = true;
    public bool Hazardous { get; set; } = true;
}

public interface IFilterablePanel
{
    void Filter(WeatherSettingsDialogFilter filter);
}
