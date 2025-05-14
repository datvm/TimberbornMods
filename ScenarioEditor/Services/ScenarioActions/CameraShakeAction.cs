namespace ScenarioEditor.Services.ScenarioActions;

public class CameraShakeAction : IScenarioAction
{
    public string NameKey { get; } = "LV.ScE.CameraShakeAction";

    public float Duration { get; set; } = 1f;
    public int Strength { get; set; } = 2;

#nullable disable
    CameraShakeService cameraShakeService;
#nullable enable

    [Inject]
    public void Inject(CameraShakeService cameraShakeService)
    {
        this.cameraShakeService = cameraShakeService;
    }

    public void Execute(ScenarioEvent scenarioEvent)
    {
        cameraShakeService.Shake(Duration, Strength);
    }

}