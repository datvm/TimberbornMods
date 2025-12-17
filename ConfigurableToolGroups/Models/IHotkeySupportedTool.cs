namespace ConfigurableToolGroups.Models;

public interface IHotkeySupportedTool
{
    IEnumerable<IToolHotkeyDefinition> GetHotkeys();
}

public interface IToolHotkeyDefinition
{
    const string DefaultGroupId = "Tools";

    string Id { get; }
    string LocKey { get; }
    string GroupId => DefaultGroupId;
    int? Order => null;

    bool IsDevTool { get; }

    void Select();
}

public class ToolHotkeyDefinitionComparer : IEqualityComparer<IToolHotkeyDefinition>
{
    public static readonly ToolHotkeyDefinitionComparer Instance = new();

    public bool Equals(IToolHotkeyDefinition x, IToolHotkeyDefinition y) => x.Id.Equals(y.Id);
    public int GetHashCode(IToolHotkeyDefinition obj) => obj.Id.GetHashCode();
}