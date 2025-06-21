namespace HydroFormaProjects.Components;

public class DamGateComponent : BaseComponent, IFinishedStateListener
{
#nullable disable
    Renderer lockRenderer;
    WaterObstacle waterObstacle;
#nullable enable

    bool hasMaterial;

    public bool Synchronize { get; set; }
    public bool Closed { get; private set; }

    public void OnEnterFinishedState()
    {
        BlockWater();
    }

    public void OnExitFinishedState()
    {
        waterObstacle.RemoveFromWaterService();
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
        AttachLock();

        if (Closed)
        {
            ToggleClosed(true);
        }
    }

    public void ToggleClosed(bool closed)
    {
        Closed = closed;
        SetVisual(closed);
        BlockWater();
    }

    void AttachLock()
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(obj.GetComponent<Collider>());
        lockRenderer = obj.GetComponent<Renderer>();

        var t = obj.transform;
        t.parent = GameObjectFast.transform;

        t.localScale = new(.8f, .8f,  .8f);
        t.position = new(.5f, .5f, .5f);
    }

    void SetVisual(bool closed)
    {
        if (!hasMaterial)
        {
            var renderer = GetComponentFast<Renderer>();
            if (renderer)
            {
                var material = renderer.materials.FirstOrDefault(q => q.name.Contains("Wood"));
                if (material)
                {
                    hasMaterial = true;
                    renderer.material = material;
                }
            }
        }

        lockRenderer.enabled = closed;
    }

}
