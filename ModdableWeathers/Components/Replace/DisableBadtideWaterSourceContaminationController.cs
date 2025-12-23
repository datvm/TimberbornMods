namespace ModdableWeathers.Components.Replace;

[HasPatch]
[BypassMethods([
    nameof(InitializeEntity),
    nameof(OnHazardousWeatherStarted),
    nameof(OnHazardousWeatherEnded),
])]
[ThrowMethods([
    nameof(Tick),
])]
public class DisableBadtideWaterSourceContaminationController()
    : BadtideWaterSourceContaminationController(null, null, null)
{

    [ReplaceMethod]
    public void MAwake()
    {
        DisableComponent();
    }

}
