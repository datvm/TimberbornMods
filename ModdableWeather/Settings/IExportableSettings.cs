namespace ModdableWeather.Settings;

public interface IExportableSettings
{
    string WeatherId { get; }

    string Export();
    void Import(string value);

}
