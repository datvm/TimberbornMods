namespace DirectionalDynamite.Components;

public class DirectionalDynamiteComponent : BaseComponent, IPersistentEntity, IAwakableComponent, IDuplicable<DirectionalDynamiteComponent>
{
    static readonly ComponentKey SaveKey = new(nameof(DirectionalDynamiteComponent));
    static readonly PropertyKey<int> DirectionKey = new("Direction");
    static readonly PropertyKey<bool> DoNotTriggerKey = new("DoNotTriggerNeighbor");

    public Direction3D Direction { get; set; } = Direction3D.Bottom;
    public int MaxDepth => dynamite.Depth;
    public Vector3Int Coordinates => dynamite._blockObject.Coordinates;

    public bool DoNotTriggerNeighbor { get; set; }

#nullable disable
    Dynamite dynamite;
    internal DirectionalDynamiteService service;
#nullable enable

    GameObject? indicator;

    [Inject]
    public void Inject(DirectionalDynamiteService service)
    {
        this.service = service;
    }

    public void Awake()
    {
        dynamite = GetComponent<Dynamite>();
    }

    public bool DestroyTerrain()
    {
        if (Direction == Direction3D.Bottom || !dynamite) { return true; } // Just the usual behavior, run the original method

        var blocks = service.GetDestroyingTerrains(this);
        service.DestroyTerrains(blocks);

        return false;
    }

    public void ShowIndicator(Sprite arrow)
    {
        service.HighlightDestroyingEntities(this);

        if (!indicator)
        {
            AttachIndicator(arrow);
        }

        if (Direction is Direction3D.Bottom or Direction3D.Top)
        {
            indicator!.SetActive(false);
            return;
        }

        indicator!.SetActive(true);
        indicator.transform.rotation = Quaternion.Euler(90, 0, Direction switch
        {
            Direction3D.Up => 180,
            Direction3D.Right => 270,
            Direction3D.Down => 0,
            Direction3D.Left => 90,
            _ => 0
        });
    }

    public void HideIndicator()
    {
        service.UnhighlightDestroyingEntities();

        if (indicator) { indicator.SetActive(false); }
    }

    static readonly Vector2 TargetIndicatorSize = new(1, 1);
    void AttachIndicator(Sprite arrow)
    {
        indicator = new GameObject();
        var t = indicator.transform;
        t.parent = GameObject.transform;
        t.localPosition = new(.5f, .1f, .5f);

        var sr = indicator.AddComponent<SpriteRenderer>();
        sr.sprite = arrow;

        var bounds = arrow.bounds.size;
        t.localScale = new(TargetIndicatorSize.x / bounds.x, TargetIndicatorSize.y / bounds.y, 1);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(DirectionKey))
        {
            Direction = (Direction3D)s.Get(DirectionKey);
        }

        if (s.Has(DoNotTriggerKey))
        {
            DoNotTriggerNeighbor = s.Get(DoNotTriggerKey);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(DirectionKey, (int)Direction);
        s.Set(DoNotTriggerKey, DoNotTriggerNeighbor);
    }

    public void Deconstruct(out Vector3Int coordinates, out int maxDepth, out Direction3D direction)
    {
        coordinates = Coordinates;
        maxDepth = MaxDepth;
        direction = Direction;
    }

    public void DuplicateFrom(DirectionalDynamiteComponent source)
    {
        Direction = source.Direction;
        DoNotTriggerNeighbor = source.DoNotTriggerNeighbor;
    }
}
