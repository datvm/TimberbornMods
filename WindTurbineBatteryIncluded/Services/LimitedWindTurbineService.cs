namespace WindTurbineBatteryIncluded.Services;

[BindSingleton]
public class LimitedWindTurbineService(WindService wind, NonlinearAnimationManager animation)
{
    const float YawSpeed = 80f;

    public readonly WindService Wind = wind;
    public readonly NonlinearAnimationManager Animation = animation;

    public readonly record struct WindHorsepower(float Strength, float Direction, float Alignment);

    public WindHorsepower Horsepower(int nominal, float yawErrorDegrees, LimitedWindTurbineSpec spec)
    {
        float strength = Wind.WindStrength > spec.MinRequiredWindStrength ? Wind.WindStrength : 0f;
        float alignment = AlignmentFactor(yawErrorDegrees, spec);
        return new(nominal * strength, nominal * strength * alignment, alignment);
    }

    public float OutputMultiplier(float yawErrorDegrees, LimitedWindTurbineSpec spec)
    {
        if (Wind.WindStrength <= spec.MinRequiredWindStrength)
        {
            return 0f;
        }

        return Wind.WindStrength * AlignmentFactor(yawErrorDegrees, spec);
    }

    public float AlignmentFactor(float yawErrorDegrees, LimitedWindTurbineSpec spec)
    {
        float error = Mathf.Max(0f, yawErrorDegrees - spec.MaxDegree);
        return Mathf.Max(spec.MinPowerMultiplier, Mathf.Cos(error * Mathf.Deg2Rad));
    }

    public float YawStep(float deltaTime)
    {
        return deltaTime * Animation.SpeedMultiplier * Wind.WindStrength * YawSpeed;
    }

    public float SignedYaw(Quaternion restRotation)
    {
        return Mathf.DeltaAngle(YawOf(restRotation), Vector2.SignedAngle(Vector2.down, Wind.WindDirection));
    }

    public float ClampedYaw(Quaternion restRotation, float maxDegree)
    {
        return Mathf.Clamp(SignedYaw(restRotation), -maxDegree, maxDegree);
    }

    public Quaternion YawRotation(Quaternion restRotation, float yaw)
    {
        return Quaternion.AngleAxis(YawOf(restRotation) + yaw, Vector3.up);
    }

    static float YawOf(Quaternion rotation)
    {
        Vector3 forward = rotation * Vector3.forward;
        return Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
    }
}
