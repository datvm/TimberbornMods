namespace PausableSensors.Components;

public class AutomatorPausable : BaseComponent, IFinishedPausable, IAwakableComponent
{
    public static readonly ConditionalWeakTable<Automator, AutomatorPausable> PausedAutomators = [];

    BlockableObject blockableObject = null!;
    Automator automator = null!;

    public void Awake()
    {
        blockableObject = GetComponent<BlockableObject>();
        automator = GetComponent<Automator>();

        blockableObject.ObjectBlocked += OnBlocked;
        blockableObject.ObjectUnblocked += OnUnblocked;
    }

    void OnUnblocked(object sender, EventArgs e)
    {
        PausedAutomators.Remove(automator);
        ForceSetState();
    }

    void OnBlocked(object sender, EventArgs e)
    {
        PausedAutomators.Add(automator, this);
        ForceSetState();        
    }

    void ForceSetState()
    {
        var currStage = automator._state;
        automator._state = currStage == AutomatorState.On ? AutomatorState.Off : AutomatorState.On;
        automator.SetStateInternal(currStage);
    }

}

