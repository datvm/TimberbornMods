namespace ConfigurableToolGroups.Specs;

#nullable disable
public record CustomBottomBarElementSpec : ComponentSpec
{

    [Serialize]
    public string Id { get; init; }

    [Serialize]
    public int Order { get; init; }

}
#nullable enable