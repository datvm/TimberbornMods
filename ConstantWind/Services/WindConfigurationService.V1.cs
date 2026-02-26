namespace ConstantWind.Services;
public class WindConfigurationService(
    WindService windService,
    MSettings s
) : ILoadableSingleton
{

    public void Load()
    {
        s.ModSettingChanged += (_, _) => OnSettingChanged();
        OnSettingChanged();
    }

    void OnSettingChanged()
    {
        var str = s.WindStrength.Value / 100f;
        windService._windServiceSpec = windService._windServiceSpec with
        {
            MinWindStrength = str,
            MaxWindStrength = str,
        };

        windService.ChangeWind();
    }
}
