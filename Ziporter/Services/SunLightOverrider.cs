namespace Ziporter.Services;

public class SunLightOverrider(Sun sun) : ILoadableSingleton, IUnloadableSingleton, IUpdatableSingleton
{

    public static SunLightOverrider? Instance { get; private set; }

    public bool Override { get; private set; }
    public OverrideParameters Parameters { get; private set; }
    public float CurrentTime { get; private set; }
    public DayStageColorsSpec? DayStageColorsSpec { get; private set; }

    public void OverrideSunLight(OverrideParameters parameters)
    {
        CurrentTime = 0;
        Parameters = parameters;
        DayStageColorsSpec = sun._sunSpec.SunsetColors with
        {
            SunColor = parameters.Color,
            SunIntensity = 30f,
            SunXAngle = 45,
        };
        Override = true;
    }

    public void UpdateSingleton()
    {
        if (!Override) { return; }

        CurrentTime += Time.deltaTime;
        if (CurrentTime >= Parameters.Duration)
        {
            Override = false;
            CurrentTime = 0;
        }
    }

    public void Load()
    {
        Instance = this;
    }

    public void Unload()
    {
        Instance = null;
    }

}

public readonly record struct OverrideParameters(Color Color, float Duration);