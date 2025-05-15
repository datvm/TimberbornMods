namespace ScenarioEditor.Models;

public readonly struct GameObjectDefinition
{
    public static readonly GameObjectDefinition BeaversOnly = new()
    {
        AdultBeaver = true,
        ChildBeaver = true,
    };

    public static readonly GameObjectDefinition CharactersOnly = BeaversOnly with { Bot = true, };

    public static readonly GameObjectDefinition BlockObjectsOnly = new() { AnyBlockObject = true, };

    public bool AdultBeaver { get; init; }
    public bool ChildBeaver { get; init; }
    public bool Bot { get; init; }
    
    public bool AnyBlockObject { get; init; }
    public bool FinishedOnly { get; init; }

    public string? SpecificObject { get; init; }
}
