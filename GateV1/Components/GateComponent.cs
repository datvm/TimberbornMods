namespace GateV1.Components;

public record GateComponentSpec : ComponentSpec;

public class GateComponent : BaseComponent, IPersistentEntity, IFinishedStateListener, IAwakableComponent, IStartableComponent
{
    static readonly ComponentKey SaveKey = new("Gate");
    static readonly PropertyKey<bool> ClosedKey = new("Close");
    static readonly PropertyKey<bool> AutoCloseHazKey = new("AutoCloseHaz");
    static readonly PropertyKey<bool> AutoCloseBadtideKey = new("AutoCloseBadtide");

#nullable disable
    BlockObjectNavMesh blockObjectNavMesh;
    BlockObject blockObject;
    FloodgateAnimationController floodgateAnimationController;
    WaterObstacle waterObstacle;
#nullable enable

    public bool IsFinished { get; private set; }
    public bool Closed { get; private set; }
    public bool AutoCloseHaz { get; set; }
    public bool AutoCloseBadtide { get; set; }

    public void Awake()
    {
        blockObjectNavMesh = GetComponent<BlockObjectNavMesh>();
        blockObject = GetComponent<BlockObject>();
        floodgateAnimationController = GetComponent<FloodgateAnimationController>();
        waterObstacle = GetComponent<WaterObstacle>();
    }

    public void Start()
    {
        if (Closed)
        {
            ToggleClosedState(true);
        }
    }

    public void ToggleClosedState(bool closed)
    {
        if (!IsFinished) { return; }

        Closed = closed;

        ToggleNavMesh(closed);
        ToggleGatePosition(closed);
        ToggleWaterObstacle(closed);
    }

    void ToggleNavMesh(bool closed)
    {
        if (blockObject.IsPreview) { return; }

        if (closed)
        {
            blockObjectNavMesh.NavMeshObject.EnqueueRemoveFromRegularNavMesh();
        }
        else
        {
            blockObjectNavMesh.RecalculateNavMeshObject();
            blockObjectNavMesh.NavMeshObject.EnqueueAddToRegularNavMesh();
        }
    }

    void ToggleGatePosition(bool closed)
    {
        floodgateAnimationController.MoveGateSmoothly(closed ? 1f : 0f);
    }

    void ToggleWaterObstacle(bool closed)
    {
        waterObstacle.RemoveFromWaterService();
        if (closed)
        {
            waterObstacle.AddToWaterService(1f);
        }
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        Closed = s.Has(ClosedKey) && s.Get(ClosedKey);
        AutoCloseHaz = s.Has(AutoCloseHazKey) && s.Get(AutoCloseHazKey);
        AutoCloseBadtide = s.Has(AutoCloseBadtideKey) && s.Get(AutoCloseBadtideKey);
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);

        s.Set(ClosedKey, Closed);
        s.Set(AutoCloseHazKey, AutoCloseHaz);
        s.Set(AutoCloseBadtideKey, AutoCloseBadtide);
    }

    public void OnEnterFinishedState()
    {
        IsFinished = true;

        if (Closed)
        {
            ToggleWaterObstacle(true);
        }
    }

    public void OnExitFinishedState()
    {
        IsFinished = false;
    }
}
