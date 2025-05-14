namespace ScenarioEditor.DevModules;

public class ScenarioEditorDevModule(CameraShakeService cameraShakeService) : IDevModule
{

    public DevModuleDefinition GetDefinition()
    {
        var builder = new DevModuleDefinition.Builder();

        for (int i = 1; i < 6; i++)
        {
            builder.AddMethod(DevMethod.Create(Prefix("Camera shake " + i), () => TestCameraShake(i)));
        }
        


        return builder.Build();
    }

    static string Prefix(string name) => $"{nameof(ScenarioEditor)}: {name}";

    void TestCameraShake(float strength)
    {
        cameraShakeService.Shake(3f, strength);
    }

}
