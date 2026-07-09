namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(ConveyorBeltComponent))]
public class ConveyorBeltProcessor(ConveyorBeltService service) : TickableComponent, IAwakableComponent, IFinishedStateListener
{
    ConveyorBeltComponent belt = null!;
    MechanicalBuilding mechanicalBuilding = null!;
    StatusToggle stuckStatusToggle = null!;

    public void Awake()
    {
        belt = GetComponent<ConveyorBeltComponent>();
        mechanicalBuilding = GetComponent<MechanicalBuilding>();
        DisableComponent();

        var t = service.t;
        stuckStatusToggle = StatusToggle.CreateNormalStatusWithAlert("LackOfResources", t.T("LV.CBlt.StuckStatus"), t.T("LV.CBlt.StuckStatusShort"), 1f);
        GetComponent<StatusSubject>().RegisterStatus(stuckStatusToggle);
    }

    public void OnEnterFinishedState()
    {
        EnableComponent();
    }

    public void OnExitFinishedState()
    {
        DisableComponent();
        stuckStatusToggle.Deactivate();
    }

    public override void Tick()
    {
        if (!mechanicalBuilding.CanUse)
        {
            stuckStatusToggle.Deactivate();
            return;
        }

        if (belt.Items.Count > 0)
        {
            MoveItems();
        }
        else
        {
            stuckStatusToggle.Deactivate();
        }

        if (belt.CanAcceptPotentialItem())
        {
            service.TryGrabContentIntoBelt(belt);
        }
    }

    void MoveItems()
    {
        var positionDelta = service.HoursPerTick / belt.Spec.TravelTimeHours;
        var itemSpace = belt.ItemSpace;
        var prev = belt.EndPosition;
        var hasStuck = false;

        for (int i = 0; i < belt.Items.Count; i++)
        {
            var item = belt.Items[i];
            if (item.Position < prev)
            {
                item.Position = Math.Min(prev, item.Position + positionDelta);
                item.Stuck = false;
            }
            else
            {
                item.Stuck = true;
                hasStuck = true;
            }

            if (i == 0 && item.Position >= belt.EndPosition)
            {
                if (service.TryTransferContentOut(belt, item.GoodId))
                {
                    i--;
                    continue;
                }
            }

            prev = item.Position - itemSpace;
        }

        if (hasStuck)
        {
            stuckStatusToggle.Activate();
        }
        else
        {
            stuckStatusToggle.Deactivate();
        }
    }
}
