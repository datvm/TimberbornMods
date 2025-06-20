namespace FileFinder;

public static class Utils
{

    public static string? ParseForGuid(string line)
    {
        if (!line.Contains(' ')) { return line; }

        var start = line.IndexOf("guid: ");
        if (start > -1)
        {
            var actualStart = start + "guid: ".Length;
            var end = line.IndexOf(',', actualStart + 1);

            var guid = line[actualStart..end];
            return guid;
        }

        return null;
    }

}
