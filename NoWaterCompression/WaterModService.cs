namespace NoWaterCompression;

public class WaterModService(WaterSimulator sim, MSettings s) : ILoadableSingleton
{

    float? gameValue;

    public void Load()
    {
        s.OnValueChanged += OnValueChanged;
        OnValueChanged();
    }

    void OnValueChanged()
    {
        gameValue ??= sim._deltaTime;

        sim._deltaTime = gameValue.Value / MSettings.MaxCurrentMultiplier;
    }

}
