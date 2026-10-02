namespace ConveyorBelt.Services;

[BindSingleton]
public class BeltRunnerMotion(IDayNightCycle day, ConveyorBeltSettings settings) : ILoadableSingleton, IUpdatableSingleton
{
    float[] along = [];
    float merger;

    public void Load() => along = new float[settings.Count];

    public float Along(int speed) => along[speed];

    public float MergerAlong => merger;

    public void UpdateSingleton()
    {
        var hours = day.SecondsToHours(Time.deltaTime);
        for (var i = 0; i < along.Length; i++)
        {
            along[i] = Mathf.Repeat(along[i] + BeltRates.Delta(settings.ItemsPerHour(i), hours), 1f);
        }

        if (along.Length == 0)
        {
            return;
        }

        var speed = along.Length > 1 ? 1 : 0;
        merger = Mathf.Repeat(merger + BeltRates.Delta(settings.ItemsPerHour(speed), hours), 1f);
    }
}
