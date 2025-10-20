namespace Ziporter.Components;

public partial class ZiporterStabilizer : TickableComponent, IDeletableEntity, IFinishedStateListener
{
    public const float StabilizerRecoveryRate = .2f;
    public const float StabilizerMax = 60f;

    public bool StabilizerNeeded { get; private set; }

    #region Inject

#nullable disable
    MechanicalNode mechNode;

    CameraShakeService cameraShakeService;
    EntityService entityService;
    IBlockService blockService;
    MapIndexService mapIndexService;
    SunLightOverrider sunLightOverrider;
    DestructionService destructionService;
#nullable enable

    [Inject]
    public void Inject(
        CameraShakeService cameraShakeService,
        EntityService entityService,
        IBlockService blockService,
        DestructionService destructionService,
        MapIndexService mapIndexService,
        SunLightOverrider sunLightOverrider
    )
    {
        this.cameraShakeService = cameraShakeService;
        this.entityService = entityService;
        this.blockService = blockService;
        this.destructionService = destructionService;
        this.mapIndexService = mapIndexService;
        this.sunLightOverrider = sunLightOverrider;
    }

    public void Awake()
    {
        mechNode = GetComponentFast<MechanicalNode>();
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
    }

    public override void Tick()
    {
        if (IsExploding || !IsEverFinished) { return; }

        StabilizerNeeded = !mechNode.Powered || mechNode.PowerInput < mechNode._nominalPowerInput;

        if (StabilizerNeeded)
        {
            Stabilizer -= Time.fixedDeltaTime;

            IsStabilizerCharging = false;

            if (Stabilizer <= 0)
            {
                Stabilizer = 0;
                OnStabilizerFailed();
            }
        }
        else if (Stabilizer < StabilizerMax)
        {
            var add = Time.fixedDeltaTime * StabilizerRecoveryRate;
            Stabilizer = Mathf.Clamp(Stabilizer + add, 0, StabilizerMax);
            IsStabilizerCharging = true;
        }
    }

    public void SetPercent(int perc)
    {
        Stabilizer = Mathf.Clamp(perc / 100f * StabilizerMax, 0, StabilizerMax);
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
        if (IsEverFinished) { return; }

        IsEverFinished = true;
        Stabilizer = StabilizerMax;
    }

    public void OnExitFinishedState() { }
}
