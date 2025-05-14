namespace ScenarioEditor.Models;

public class ScenarioEvent(string name)
{
    public const int InfiniteRepeat = -1;

    public int Id { get; set; }

    public string Name { get; set; } = name;
    public string Note { get; set; } = "";

    public bool IsEnabled { get; set; }
    
    public bool IsInProgress => IsEnabled && !IsCompleted;
    public bool IsCompleted => Repeat != InfiniteRepeat && TriggerCount >= Repeat;

    public List<IScenarioTrigger> Triggers { get; init; } = [];
    public bool AllTriggersRequired { get; init; }
    public int TriggerCount { get; set; }

    public List<IScenarioAction> Actions { get; init; } = [];
    public int Repeat { get; set; }
}

public interface IUIIdentifiable
{
    string NameKey { get; }
}

public interface IScenarioTrigger : IUIIdentifiable
{

    bool Check(ScenarioEvent scenarioEvent);

}

public interface IScenarioAction : IUIIdentifiable
{

    void Execute(ScenarioEvent scenarioEvent);

}

public readonly record struct OnScenarioEventTriggered(ScenarioEvent Event);
public readonly record struct OnScenarioEventChanged(ScenarioEvent Event);