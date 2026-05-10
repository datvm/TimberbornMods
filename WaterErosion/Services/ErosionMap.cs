
namespace WaterErosion.Services;

[BindSingleton]
public class ErosionMap(
    MapIndexService mapIndexService,
    ISingletonLoader singletonLoader
) : ISaveableSingleton, ILoadableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(ErosionMap));
    static readonly PropertyKey<string> ValuesKey = new("Values");

    public const float DefaultErosion = 1f;
    public float[] Map { get; private set; } = [];

    public float GetErosion(Vector3Int coordinates) 
        => Map[mapIndexService.CoordinatesToIndex3D(coordinates)];
    public void SetErosion(Vector3Int coordinates, float value) 
        => Map[mapIndexService.CoordinatesToIndex3D(coordinates)] = Mathf.Clamp01(value);

    public void Save(ISingletonSaver singletonSaver)
    {
        singletonSaver.GetSingleton(SaveKey).Set(ValuesKey, WaterErosionUtils.SerializeFloatArray(Map));
    }

    public void Load()
    {
        if (!TryLoadSavedData())
        {
            Initialize();
        }
    }

    bool TryLoadSavedData()
    {
        if (singletonLoader.TryGetSingleton(SaveKey, out var s))
        {
            Map = WaterErosionUtils.DeserializeFloatArray(s.Get(ValuesKey));

            if (Map.Length != mapIndexService.MaxSize3D)
            {
                Debug.LogWarning($"ErosionMap: Loaded map has incorrect size ({Map.Length} instead of {mapIndexService.MaxSize3D}). Reinitializing.");
                return false;
            }

            return true;
        }

        return false;
    }

    void Initialize()
    {
        Map = new float[mapIndexService.MaxSize3D];
        Array.Fill(Map, DefaultErosion);
    }


}
