namespace TimberModBuilder.Services;

public class ModBuilderLocalization(string loc)
{

    public string Localization { get; } = loc;
    public readonly Dictionary<string, string> Values = [];

    public string ToCsv()
    {
        StringBuilder str = new();

        str.AppendLine("ID,Text,Comment");

        foreach (var (k, v) in Values)
        {
            str.AppendLine($"{k},{EscapeCsv(v)},");
        }

        return str.ToString().Trim();
    }

    public static string EscapeCsv(string str)
        => $"\"{str.Replace("\"", "\"\"")}\"";

}
