namespace HydroFormaProjects.Components;

public class DamGateComponentSpec : BaseComponent { }

public class DamGateComponent : BaseComponent, IFinishedStateListener, IPersistentEntity
{
    static readonly Shader LockShader = Shader.Find("Shader Graphs/EnvironmentURP");
    static readonly Color LockColor = new(118 / 255f, 107 / 255f, 99 / 255f);

    static readonly ComponentKey SaveKey = new("DamGate");
    static readonly PropertyKey<bool> ClosedKey = new("Closed");
    static readonly PropertyKey<bool> SynchronizeKey = new("Synchronize");

#nullable disable
    WaterObstacle waterObstacle;
#nullable enable

    GameObject? lockObj;

    public bool Synchronize { get; set; } = true;
    public bool Closed { get; private set; }

    public bool Finished { get; private set; }
    bool initialized;

    public void OnEnterFinishedState()
    {
        AttachLock();
        Finished = true;

        if (initialized)
        {
            ToggleClosed(Closed);
        }
    }

    public void OnExitFinishedState()
    {
        Finished = false;
        waterObstacle.RemoveFromWaterService();
        DestroyLock();
    }

    void BlockWater()
    {
        waterObstacle.RemoveFromWaterService();
        waterObstacle.AddToWaterService(Closed ? 1f : .65f);
    }

    public void Awake()
    {
        waterObstacle = GetComponentFast<WaterObstacle>();
    }

    public void Start()
    {
        initialized = true;
        ToggleClosed(Closed);
    }

    public void ToggleClosed(bool closed)
    {
        if (!Finished || !initialized) { return; }

        Closed = closed;
        SetVisual(closed);
        BlockWater();
    }

    void AttachLock()
    {
        var obj = lockObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(obj.GetComponent<Collider>());

        var renderer = obj.GetComponent<Renderer>();
        renderer.material.shader = LockShader;
        renderer.material.SetColor("_Color", LockColor);

        var t = obj.transform;
        t.parent = GameObjectFast.transform;

        t.localScale = new(.8f, .8f, .8f);
        t.localPosition = new(.5f, .5f, .5f);

        obj.SetActive(false);
    }

    void DestroyLock()
    {
        if (lockObj)
        {
            Destroy(lockObj);
            lockObj = null;
        }
    }

    void SetVisual(bool closed)
    {
        if (!lockObj) { return; }
        lockObj.SetActive(closed);
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);

        s.Set(ClosedKey, Closed);
        s.Set(SynchronizeKey, Synchronize);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(ClosedKey))
        {
            Closed = s.Get(ClosedKey);
        }

        if (s.Has(SynchronizeKey))
        {
            Synchronize = s.Get(SynchronizeKey);
        }
    }
}
