namespace ScienceShop.Components;

[AddTemplateModule2(typeof(ConsumeScienceManufactorySpec))]
public class WaitingForScienceStatus(ILoc t) : TickableComponent, IAwakableComponent, IInitializableEntity, IFinishedStateListener
{
    const string Icon = "NotEnoughScience";

    ConsumeScienceManufactory shop = null!;
    Workplace workplace = null!;
    StatusToggle status = null!;

    public void Awake()
    {
        shop = GetComponent<ConsumeScienceManufactory>();
        workplace = GetComponent<Workplace>();
        status = StatusToggle.CreateNormalStatusWithAlertAndFloatingIcon(
            Icon,
            t.T("LV.ScS.StatusNotEnoughScience"),
            t.T("LV.ScS.StatusNotEnoughScience.Short"),
            0.2f);
        DisableComponent();
    }

    public void InitializeEntity() => GetComponent<StatusSubject>().RegisterStatus(status);

    public void OnEnterFinishedState()
    {
        EnableComponent();
        UpdateStatus();
    }

    public void OnExitFinishedState()
    {
        status.Deactivate();
        DisableComponent();
    }

    public override void Tick() => UpdateStatus();

    void UpdateStatus()
    {
        if (shop.WaitingForScience && Staffed())
        {
            status.Activate();
        }
        else
        {
            status.Deactivate();
        }
    }

    bool Staffed()
    {
        if (!workplace)
        {
            return true;
        }

        return workplace.NumberOfAssignedWorkers > 0;
    }
}
