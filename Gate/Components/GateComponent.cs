namespace Gate.Components;

public class GateComponent : BaseComponent, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new("Gate");
    static readonly PropertyKey<bool> ClosedKey = new("Close");

#nullable disable
    BlockObjectNavMesh blockObjectNavMesh;
    BlockObject blockObject;
    FloodgateAnimationController floodgateAnimationController;
#nullable enable

    public bool Closed { get; private set; }

    public void Awake()
    {
        blockObjectNavMesh = GetComponentFast<BlockObjectNavMesh>();
        blockObject = GetComponentFast<BlockObject>();
        floodgateAnimationController = GetComponentFast<FloodgateAnimationController>();
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
        Closed = closed;

        ToggleNavMesh(closed);
        ToggleGatePosition(closed);
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

    public void Load(IEntityLoader entityLoader)
    {
        Closed = entityLoader.TryGetComponent(SaveKey, out var s)
            && s.Has(ClosedKey) && s.Get(ClosedKey);
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (Closed)
        {
            var s = entitySaver.GetComponent(SaveKey);
            s.Set(ClosedKey, Closed);
        }
    }
}
