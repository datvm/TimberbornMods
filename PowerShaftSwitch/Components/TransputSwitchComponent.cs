namespace PowerShaftSwitch.Components;

public class TransputSwitchComponentSpec : BaseComponent { }

public class TransputSwitchComponent : BaseComponent, IPausableComponent
{
#nullable disable
    PausableBuilding pausableBuilding;
    MechanicalNode mechNode;
    EntityComponent entityComponent;
#nullable enable

    bool mechNodeFinished;
    bool isAdded;

    public void Awake()
    {
        pausableBuilding = GetComponentFast<PausableBuilding>();
        mechNode = GetComponentFast<MechanicalNode>();
        entityComponent = GetComponentFast<EntityComponent>();
    }

    public void Start()
    {
        pausableBuilding.PausedChanged += OnPauseChanged;
    }

    public void OnMechNodeEnterFinishedState()
    {
        isAdded = true;
        mechNodeFinished = true;
        if (!pausableBuilding.Paused) { return; }

        RemoveMechNode();
    }

    public bool OnMechNodeExitFinishedState()
    {
        mechNodeFinished = false;

        return isAdded;
    }

    private void OnPauseChanged(object sender, EventArgs e)
    {
        if (!mechNodeFinished || entityComponent.Deleted) { return; }

        if (pausableBuilding.Paused)
        {
            RemoveMechNode();
        }
        else
        {
            AddMechNode();
        }
    }

    void RemoveMechNode()
    {
        if (!isAdded) { return; }

        mechNode._transputMap.RemoveNode(mechNode);
        mechNode._mechanicalGraphManager.RemoveNode(mechNode);
        isAdded = false;
    }

    void AddMechNode()
    {
        if (isAdded) { return; }

        mechNode.OnEnterFinishedState(); // This is safe to call for now, and to raise the event
        isAdded = true;
    }
}
