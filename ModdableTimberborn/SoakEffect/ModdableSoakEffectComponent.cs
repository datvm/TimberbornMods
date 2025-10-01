namespace ModdableTimberborn.SoakEffect;

/// <summary>
/// Component that tracks if a character is in water and raises events accordingly.
/// </summary>
public class ModdableSoakEffectComponent : TickableComponent
{

    /// <summary>
    /// Raised each tick when the character is in water.
    /// Note that you should check if the character is resistant to water via <see cref="SoakEffectEventArgs.Resistant"/>.
    /// </summary>
    public event EventHandler<SoakEffectEventArgs>? OnSoakedTick;

    /// <summary>
    /// Raised when the character's water state changes (enters or exits water).
    /// </summary>
    public event EventHandler<bool>? IsInWaterStateChanged;

    readonly List<IWaterResistor> waterResistors = [];
    ThreadSafeWaterMap waterMap = null!;
    IDayNightCycle dayNightCycle = null!;

    /// <summary>
    /// True if the character is currently in water and not resistant to it.
    /// </summary>
    public bool IsInWater { get; private set; }

    [Inject]
    public void Inject(IThreadSafeWaterMap waterMap, IDayNightCycle dayNightCycle)
    {
        this.waterMap = (ThreadSafeWaterMap)waterMap;
        this.dayNightCycle = dayNightCycle;
    }

    public void Awake()
    {
        GetComponentsFast(waterResistors);
    }

    public override void StartTickable()
    {
        UpdateState();
    }

    public override void Tick()
    {
        var prevInWater = IsInWater;

        var e = UpdateState();
        if (IsInWater != prevInWater)
        {
            IsInWaterStateChanged?.Invoke(this, IsInWater);
        }

        if (e is not null)
        {
            OnSoakedTick?.Invoke(this, e.Value);
        }
    }

    SoakEffectEventArgs? UpdateState()
    {
        IsInWater = false;

        var resistant = waterResistors.FastAny(q => q.IsWaterResistant);
        var coord = NavigationCoordinateSystem.WorldToGridInt(TransformFast.position);

        var column = waterMap.GetColumn(coord);
        if (column.WaterDepth <= 0) { return null; }

        var waterCeil = Mathf.CeilToInt(column.WaterDepth + column.Floor);
        if (waterCeil < coord.z) { return null; }

        IsInWater = !resistant;
        var passedTime = dayNightCycle.FixedDeltaTimeInHours;
        return new(resistant, column, coord, waterCeil, passedTime);
    }

}

public readonly record struct SoakEffectEventArgs(
    bool Resistant,
    in ReadOnlyWaterColumn WaterColumn,
    Vector3Int Coordinate,
    int WaterCeiling,
    float PassedTimeInHours
);