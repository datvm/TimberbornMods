namespace ScenarioEditor.Services;

public class CameraShakeService(
    MSettings s,
    CameraService cameraService
) : IUpdatableSingleton
{
    bool isShaking;
    float duration;
    float amplitude;

    Vector3 originalPos;
    Vector2 originalRot;

    public void Shake(float duration, float strength)
    {
        if (!s.CameraShake.Value) { return; }

        this.duration = duration;
        amplitude = .05f * strength;

        if (!isShaking)
        {
            originalPos = cameraService.Target;
            originalRot = new(cameraService.VerticalAngle, cameraService.HorizontalAngle);
        }
        isShaking = true;
    }

    public void UpdateSingleton()
    {
        if (!isShaking) { return; }

        duration -= Time.unscaledDeltaTime;
        if (duration <= 0f)
        {
            StopShaking();
            return;
        }

        var offset = UnityEngine.Random.insideUnitSphere * amplitude;
        cameraService.Target = originalPos + offset;

        float tilt = amplitude;
        cameraService.VerticalAngle = originalRot.x + UnityEngine.Random.Range(-tilt, tilt);
        cameraService.HorizontalAngle = originalRot.y + UnityEngine.Random.Range(-tilt, tilt);
    }

    public void StopShaking()
    {
        cameraService.Target = originalPos;
        cameraService.VerticalAngle = originalRot.x;
        cameraService.HorizontalAngle = originalRot.y;

        isShaking = false;
    }
}
