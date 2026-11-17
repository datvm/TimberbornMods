namespace MechanizedManufactory.Components;

[AddTemplateModule2(typeof(MechanizedManufactorySpec))]
public class MechanizedManufactoryComponent(IDayNightCycle dayNightCycle, ILoc t, MSettings s) : TickableComponent, IAwakableComponent, IInitializableEntity, IFinishedStateListener, IEntityDescriber
{
    // Above WorkerTypeIlluminator's 20, so this replaces the beaver and bot door colors.
    const int DoorColorPriority = 30;

    MechanizedManufactorySpec spec = null!;
    public int Workers => spec.Workers;

    BlockableObject blockable = null!;
    Manufactory manufactory = null!;
    Workshop workshop = null!;
    Illuminator? illuminator;
    IlluminatorToggle? doorLight;

    public bool IsWorking { get; private set; }

    NoHaulingPostStatus noHaulingPostStatus = null!;

    public void Awake()
    {
        manufactory = GetComponent<Manufactory>();
        spec = GetComponent<MechanizedManufactorySpec>();
        blockable = GetComponent<BlockableObject>();
        noHaulingPostStatus = GetComponent<NoHaulingPostStatus>();
        workshop = GetComponent<Workshop>();
        illuminator = GetComponent<Illuminator>();
        DisableComponent();
    }

    public void InitializeEntity()
    {
        if (illuminator is null) { return; }

        doorLight = illuminator.CreateToggle();
        illuminator.CreateColorizer(DoorColorPriority).SetColor(s.DoorColor.Color);
    }

    public IEnumerable<EntityDescription> DescribeEntity() => [
        EntityDescription.CreateTextSection(
            t.T("LV.MM.BuildingDescriber", ManufactoryCopier.GetPower(Workers), Workers),
            2030)];

    public void OnEnterFinishedState()
    {
        blockable.ObjectBlocked += OnObjectBlockedChanged;
        blockable.ObjectUnblocked += OnObjectBlockedChanged;
        OnObjectBlockedChanged(null!, EventArgs.Empty);

        noHaulingPostStatus.Initialize(() => true);
    }

    public void OnExitFinishedState()
    {
        blockable.ObjectBlocked -= OnObjectBlockedChanged;
        blockable.ObjectUnblocked -= OnObjectBlockedChanged;
        DisableComponent();
    }

    void OnObjectBlockedChanged(object sender, EventArgs e)
    {
        if (blockable.IsUnblocked)
        {
            EnableComponent();
        }
        else
        {
            DisableComponent();
            ChangeStatus(false);
        }
    }

    public override void Tick()
    {
        if (!manufactory.IsReadyToProduce)
        {
            ChangeStatus(false);
            return;
        }

        ChangeStatus(true);
        manufactory.IncreaseProductionProgress(dayNightCycle.FixedDeltaTimeInHours * spec.Workers);
    }

    void ChangeStatus(bool working)
    {
        if (IsWorking == working) { return; }
        IsWorking = working;
        doorLight?.Toggle(working);
        workshop.InvokeWorkshopStateChangedEvent();
    }

}
