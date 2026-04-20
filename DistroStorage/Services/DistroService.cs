namespace DistroStorage.Services;

[BindSingleton]
public class DistroService(
    DistroRegistry registry,
    MSettings settings
) : ILoadableSingleton
{

    public float TransferTime { get; private set; }
    public bool StockpileEnabledDefault { get; private set; }
    public bool DisableConstruction { get; private set; }
    public bool ConstructionEnableDefault { get; private set; }
    public bool DisableManufactory { get; private set; }
    public bool ManufactoryEnableDefault { get; private set; }

    public void Load()
    {
        // TransferRate is items per hour, transfer time should be hours per item
        TransferTime = 1f / Mathf.Max(.01f, settings.TransferRate.Value);

        StockpileEnabledDefault = settings.StockpileEnableDefault.Value;
        DisableConstruction = !settings.EnableConstruction.Value;
        ConstructionEnableDefault = !DisableConstruction && settings.ConstructionEnableDefault.Value;
        DisableManufactory = !settings.EnableManufactory.Value;
        ManufactoryEnableDefault = !DisableManufactory && settings.ManufactoryEnableDefault.Value;
    }

    public void Register(IDistroComponent component) => registry.Register(component);
    public void Unregister(IDistroComponent component) => registry.Unregister(component);

}
