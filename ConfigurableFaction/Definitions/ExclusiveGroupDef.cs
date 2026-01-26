namespace ConfigurableFaction.Definitions;

public class ExclusiveGroupDef(string Name)
{
    public string Name { get; } = Name;
    public HashSet<string> Templates { get; } = [];
}
