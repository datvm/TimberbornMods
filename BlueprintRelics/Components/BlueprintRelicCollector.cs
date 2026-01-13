namespace BlueprintRelics.Components;

public class BlueprintRelicCollector(BlueprintRelicCollectorService service) : TickableComponent, IAwakableComponent, IPersistentEntity, IInitializableEntity
{

    static readonly ComponentKey SaveKey = new(nameof(BlueprintRelicCollector));
    static readonly PropertyKey<bool> PauseCollectingKey = new("PauseCollecting");
    static readonly ListKey<string> RequiredItemsKey = new("RequiredItems");
    static readonly PropertyKey<int> ExpiryTicksKey = new("ExpiryTicks");
    static readonly PropertyKey<int> StepsLeftKey = new("StepsLeft");
    static readonly PropertyKey<int> StepTickLeftKey = new("StepTickLeft");
    static readonly PropertyKey<int> ScienceRequirementKey = new("ScienceRequirement");
    static readonly PropertyKey<float> TotalDaysKey = new("TotalDays");

#nullable disable
    BlueprintRelicSpec spec;
    DistrictBuilding districtBuilding;

    StatusToggle lackMaterialStatus, finishedStatus;
#nullable enable

    public BlueprintRelicSize Size => spec.Size;

    public bool PauseCollecting { get; set; }

    public int ExpiryTicks { get; private set; }
    public float TotalDays { get; private set; }

    public int StepsLeft { get; private set; }
    public int TotalSteps => spec.ExcavationSteps;
    public bool Finished => StepsLeft <= 0;

    public int StepTickLeft { get; private set; }
    public float StepDays => spec.ExcavationStepDays;
    public bool IsExcavating => StepTickLeft > 0;

    public ImmutableArray<GoodAmount> RequiredGoods { get; private set; } = [];
    public int ScienceRequirement { get; private set; }

    public void Awake()
    {
        spec = GetComponent<BlueprintRelicSpec>();
        districtBuilding = GetComponent<DistrictBuilding>();

        var t = service.t;
        lackMaterialStatus = StatusToggle.CreateNormalStatusWithAlert("LackOfResources", t.T("LV.BRe.StatusNoMaterial"), t.T("LV.BRe.StatusNoMaterialShort"), 2f);
        finishedStatus = StatusToggle.CreateNormalStatusWithAlert("ExcavationFinished", t.T("LV.BRe.StatusExcavationFinished"), t.T("LV.BRe.StatusExcavationFinishedShort"));

    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(PauseCollectingKey))
        {
            PauseCollecting = s.Get(PauseCollectingKey);
        }

        RequiredGoods = [.. s.GetPairs<string, int>(RequiredItemsKey)
            .Select(kv => new GoodAmount(kv.Key, kv.Value))];
        ExpiryTicks = s.Get(ExpiryTicksKey);
        StepsLeft = s.Get(StepsLeftKey);
        StepTickLeft = s.Get(StepTickLeftKey);
        ScienceRequirement = s.Get(ScienceRequirementKey);
        TotalDays = s.Get(TotalDaysKey);
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        if (PauseCollecting)
        {
            s.Set(PauseCollectingKey, true);
        }

        s.SetPairs(RequiredItemsKey, RequiredGoods
            .Select(q => new KeyValuePair<string, int>(q.GoodId, q.Amount)));

        s.Set(ExpiryTicksKey, ExpiryTicks);
        s.Set(StepsLeftKey, StepsLeft);
        s.Set(StepTickLeftKey, StepTickLeft);
        s.Set(ScienceRequirementKey, ScienceRequirement);
        s.Set(TotalDaysKey, TotalDays);
    }

    public void InitializeEntity()
    {
        GetComponent<StatusSubject>().RegisterStatuses([lackMaterialStatus, finishedStatus]);

        InitRequirements();

        if (Finished) { SetFinishState(); }
    }

    void SetFinishState()
    {
        finishedStatus.Activate();
        DisableComponent();
    }

    void InitRequirements()
    {
        if (TotalDays > 0) { return; } // Already initialized

        var expRange = spec.ExpireInDays;
        TotalDays = Random.Range((float)expRange.Min, expRange.Max);
        ExpiryTicks = Mathf.FloorToInt(TotalDays * service.TicksInDay);
        StepsLeft = TotalSteps;

        // Select requirements
        ScienceRequirement = BlueprintRelicCollectorService.RandomMinMax(spec.MaxScienceRequirement);
        RequiredGoods = service.GenerateGoodRequirements(spec);
    }

    public override void Tick()
    {
        if (ExpiryTicks <= 0 || Finished)
        {
            // Should not happen
            return;
        }

        if (IsExcavating)
        {
            StepTickLeft--;

            if (StepTickLeft <= 0)
            {
                StepTickLeft = 0;
                StepsLeft--;

                if (Finished)
                {
                    SetFinishState();
                    return;
                }
            }
        }
        else
        {
            // First try to collect required items
            if (TryCollecting())
            {
                lackMaterialStatus.Deactivate();
                return;
            }
            else if (!PauseCollecting && districtBuilding.District)
            {
                lackMaterialStatus.Activate();
            }

            ExpiryTicks--;
            if (ExpiryTicks <= 0)
            {
                service.Expires(this);
            }
        }
    }

    bool TryCollecting()
    {
        if (!service.TryCollecting(this, districtBuilding.District)) { return false; }

        StepTickLeft = Mathf.FloorToInt(StepDays * service.TicksInDay);
        return true;
    }

    public void DigAgain()
    {
        StepsLeft = TotalSteps;
        StepTickLeft = 0;

        EnableComponent();
    }

}

