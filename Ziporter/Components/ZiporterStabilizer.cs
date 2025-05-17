namespace Ziporter.Components;

public partial class ZiporterStabilizer : TickableComponent, IDeletableEntity, IFinishedStateListener
{
    public const float StabilizerMax = 60f;

    #region Inject

#nullable disable
    ZiporterBattery battery;
    StatusToggle stabilizerStatus;
    CameraShakeService cameraShakeService;
    EntityService entityService;
    IBlockService blockService;
    TerrainDestroyer terrainDestroyer;
    MapIndexService mapIndexService;
    SunLightOverrider sunLightOverrider;
    ILoc t;
#nullable enable

    [Inject]
    public void Inject(
        CameraShakeService cameraShakeService,
        EntityService entityService,
        IBlockService blockService,
        TerrainDestroyer terrainDestroyer,
        MapIndexService mapIndexService,
        SunLightOverrider sunLightOverrider,
        ILoc t
    )
    {
        this.cameraShakeService = cameraShakeService;
        this.entityService = entityService;
        this.blockService = blockService;
        this.terrainDestroyer = terrainDestroyer;
        this.mapIndexService = mapIndexService;
        this.sunLightOverrider = sunLightOverrider;
        this.t = t;
    }

    public void Awake()
    {
        battery = GetComponentFast<ZiporterBattery>();
    }

    #endregion

    public float Stabilizer { get; private set; } = StabilizerMax;
    public float StabilizerPercent => Stabilizer / StabilizerMax;
    public bool IsStabilizerCharging { get; private set; }

    public bool IsEverFinished { get; private set; }

    public void Load(float stabilizer, bool isEverFinished)
    {
        Stabilizer = Mathf.Clamp(stabilizer, 0, StabilizerMax);
        IsEverFinished = isEverFinished;

        Debug.Log("Loaded IsEverFinished: " + isEverFinished);
    }

    public override void StartTickable()
    {
        stabilizerStatus = StatusToggle.CreatePriorityStatusWithAlertAndFloatingIcon(
            "stabilizer-draining", "LV.Ziporter.WarningDraining".T(t), "LV.Ziporter.WarningDrainingShort".T(t));
        GetComponentFast<StatusSubject>().RegisterStatus(stabilizerStatus);
    }

    public override void Tick()
    {
        if (IsExploding) { return; }

        if (battery.Charge <= 0)
        {
            Stabilizer -= Time.fixedDeltaTime;
            stabilizerStatus.Activate();

            IsStabilizerCharging = false;

            if (Stabilizer <= 0)
            {
                Stabilizer = 0;
                OnStabilizerFailed();
            }
        }
        else if (Stabilizer < StabilizerMax)
        {
            stabilizerStatus.Deactivate();

            var add = Time.fixedDeltaTime;
            Stabilizer = Mathf.Clamp(Stabilizer + add, 0, StabilizerMax);
            IsStabilizerCharging = true;
        }
    }

    void OnStabilizerFailed()
    {
        entityService.Delete(this);
    }

    public void DeleteEntity()
    {
        if (!IsEverFinished) { return; }

        Explode();
    }

    public void OnEnterFinishedState()
    {
        Debug.Log("OnEnterFinishedState");

        if (IsEverFinished) { return; }

        IsEverFinished = true;
        Stabilizer = StabilizerMax;
    }

    public void OnExitFinishedState() { }
}
