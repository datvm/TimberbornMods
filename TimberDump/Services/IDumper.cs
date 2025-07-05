namespace TimberDump.Services;

public interface IDumper
{

    string? Folder { get; }
    IEnumerable<(string Name, Func<object?> Data)> GetDumpData();
}
