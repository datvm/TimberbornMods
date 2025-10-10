namespace ScientificProjects.Services;

public interface ISPDevModule
{
    IEnumerable<SPDevEntry> GetEntries();
}

public record SPDevEntry(string Name, Action Action);