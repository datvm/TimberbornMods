namespace ConfigurableToolGroups.Specs;

public record CustomRootElementSpec : ComponentSpec
{

    [Serialize]
    public RootElementLocation Location { get; init; }

}
