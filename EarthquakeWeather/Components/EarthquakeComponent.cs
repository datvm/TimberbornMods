namespace EarthquakeWeather.Components;

public class EarthquakeComponent : BaseComponent, IFinishedStateListener
{

#nullable disable
    EarthquakeRegistry earthquakeRegistry;
    BlockObject blockObject;
    BuildingHPComponent hp;
#nullable enable

    public event EventHandler<EarthquakeBuildingDamageEventArgs>? OnBeforeEarthquakeDamage;
    public event EventHandler<EarthquakeBuildingAfterDamageEventArgs>? OnAfterEarthquakeDamage;

    public bool Finished => blockObject.IsFinished;
    public Vector3Int Coordinates => blockObject.Coordinates;
    public Vector2Int CoordiantesXY => Coordinates.XY();

    public IEnumerable<Vector2Int> BlockObjectTiles =>
        PositionedBlocks.From(blockObject.Blocks, blockObject.Placement)
            .GetAllCoordinates()
            .Select(q => q.XY())
            .Distinct();

    [Inject]
    public void Inject(EarthquakeRegistry earthquakeRegistry, ILoc t)
    {
        this.earthquakeRegistry = earthquakeRegistry;
    }

    public void Awake()
    {
        blockObject = GetComponentFast<BlockObject>();
        hp = GetComponentFast<BuildingHPComponent>();
    }

    public void OnEnterFinishedState()
    {
        earthquakeRegistry.Register(this);
    }

    public void OnExitFinishedState()
    {
        earthquakeRegistry.Unregister(this);
    }

    public void Damage(int damage, EarthquakeNotificationService earthquakeNotification)
    {
        if (hp.Invulnerable || damage == 0 || !this || !blockObject.IsFinished) { return; }

        var e = new EarthquakeBuildingDamageEventArgs()
        {
            Damage = damage,
        };
        OnBeforeEarthquakeDamage?.Invoke(this, e);

        if (e.Cancel) { return; }

        if (DetonateIfDynamite())
        {
            earthquakeNotification.LogDetonatedExplosive();
            return;
        }

        var prev = hp.HP;
        hp.Damage(damage);
        var curr = hp.HP;
        if (curr >= prev || curr <= 0) { return; }

        earthquakeNotification.LogDamagedBuilding();
        OnAfterEarthquakeDamage?.Invoke(this, new(prev - curr, earthquakeNotification));
    }

    bool DetonateIfDynamite()
    {
        var dynamite = GetComponentFast<Dynamite>();
        if (!dynamite) { return false; }

        dynamite.Detonate();
        return true;
    }

}

public class EarthquakeBuildingDamageEventArgs
{
    public bool Cancel { get; set; }
    public int Damage { get; set; }
}

public record EarthquakeBuildingAfterDamageEventArgs(int Damage, EarthquakeNotificationService Notifications);