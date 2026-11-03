namespace ThePostOffice.Components;

public enum PostOfficeStall
{
    None,
    NothingToDo,
    NoStorage,
    LackOfResources,
}

public class PostOfficeStatus(ILoc t) : TickableComponent, IAwakableComponent, IInitializableEntity
{
    const float DelayHours = 0.25f;

    StatusToggle nothing = null!;
    StatusToggle storage = null!;
    StatusToggle resources = null!;
    PostOfficeSender? sender;
    PostOfficeReceiver? receiver;
    PostOfficeStall shown = PostOfficeStall.None;

    public void Awake()
    {
        TryGetComponent(out sender);
        TryGetComponent(out receiver);
    }

    public void InitializeEntity()
    {
        nothing = StatusToggle.CreateNormalStatusWithAlertAndFloatingIcon(
            "NothingToDo",
            t.T("LV.TPO.StatusNothing"),
            t.T("LV.TPO.StatusNothingShort"),
            DelayHours);
        storage = StatusToggle.CreateNormalStatusWithAlertAndFloatingIcon(
            "NoStorage",
            t.T("LV.TPO.StatusStorage"),
            t.T("LV.TPO.StatusStorageShort"),
            DelayHours);
        resources = StatusToggle.CreateNormalStatusWithAlertAndFloatingIcon(
            "LackOfResources",
            t.T("Status.Work.LackOfResources"),
            t.T("Status.Work.LackOfResources.Short"),
            DelayHours);
        var subject = GetComponent<StatusSubject>();
        subject.RegisterStatus(nothing);
        subject.RegisterStatus(storage);
        subject.RegisterStatus(resources);
        Apply();
    }

    public override void Tick()
    {
        Apply();
    }

    void Apply()
    {
        var next = Current();
        if (next == shown)
        {
            return;
        }

        shown = next;
        nothing.Toggle(next == PostOfficeStall.NothingToDo);
        storage.Toggle(next == PostOfficeStall.NoStorage);
        resources.Toggle(next == PostOfficeStall.LackOfResources);
    }

    PostOfficeStall Current()
    {
        var sending = sender;
        if (sending is not null && sending)
        {
            return sending.Stall();
        }

        var receiving = receiver;
        if (receiving is not null && receiving)
        {
            return receiving.Stall();
        }

        return PostOfficeStall.None;
    }
}
