
using ModdableWeather.Services.Registries;

namespace ModdableWeather.Services;

public class ModdableSun(
    CameraService cameraService,
    DayStageCycle dayStageCycle,
    ISpecService specService,
    RootObjectProvider rootObjectProvider,
    ModdableWeatherSpecRegistry specs
) : Sun(cameraService, dayStageCycle, specService, rootObjectProvider),
    ILoadableSingleton, IUnloadableSingleton
{

    static ModdableSun? instance;
    public static ModdableSun Instance => instance.InstanceOrThrow();

    public new void Load()
    {
        instance = this;
        base.Load();
    }

    public void Unload()
    {
        instance = null;
    }

    public void SetFogColor(DayStageTransition dayStageTransition)
    {
        var fogSettings = GetFogSettings(dayStageTransition.CurrentDayStageHazardousWeatherId, dayStageTransition.CurrentDayStage);
        var fogSettings2 = GetFogSettings(dayStageTransition.NextDayStageHazardousWeatherId, dayStageTransition.NextDayStage);
        var transitionProgress = dayStageTransition.TransitionProgress;

        RenderSettings.fogColor = Color.Lerp(fogSettings.FogColor, fogSettings2.FogColor, transitionProgress);
        RenderSettings.fogDensity = Mathf.Lerp(fogSettings.FogDensity, fogSettings2.FogDensity, transitionProgress);
    }

    FogSettingsSpec GetFogSettings(string weatherId, DayStage dayStage)
    {
        var spec = specs.GetSkySpec(weatherId);
        return spec[dayStage];
    }

}
