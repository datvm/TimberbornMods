namespace TimberDump.Services.Dumpers;

public class LocDumper(ILoc t) : IDumper
{
    readonly Loc t = (Loc)t;

    public int Order { get; }
    public string? Folder { get; }

    public void Dump(string folder)
    {
        var filePath = Path.Combine(folder, "localizations.csv");

        using var writer = new StreamWriter(filePath);
        writer.WriteLine("ID,Text,Comment");

        foreach (var (key, value) in t._localization)
        {
            writer.WriteLine(
                $"{key},{EscapeCsvValue(value)},"
            );
        }
    }

    static string EscapeCsvValue(string value) => $"\"{value.Replace("\"", "\"\"")}\"";

}
