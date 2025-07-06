namespace TimberDump.Services;

public interface IDumper
{
    public int Order { get; }
    string? Folder { get; }

    void Dump(string folder);
}

public interface IJsonDumper : IDumper
{    
    IEnumerable<(string Name, Func<object?> Data)> GetDumpData();
}
