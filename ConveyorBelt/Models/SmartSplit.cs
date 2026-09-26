namespace ConveyorBelt.Models;

public enum SmartSplitMode
{
    Any,
    Good,
    Overflow,
}

public readonly record struct SmartSplitPort(SmartSplitMode Mode, string? GoodId)
{
    public static SmartSplitPort AnyPort { get; } = new(SmartSplitMode.Any, null);

    public bool Matches(string goodId) => Mode switch
    {
        SmartSplitMode.Any => true,
        SmartSplitMode.Good => GoodId == goodId,
        SmartSplitMode.Overflow => true,
        _ => false,
    };

    public string Serialize() => Mode switch
    {
        SmartSplitMode.Good => GoodId ?? "",
        SmartSplitMode.Overflow => "*",
        _ => "",
    };

    public static SmartSplitPort Deserialize(string text)
    {
        if (text.Length == 0)
        {
            return AnyPort;
        }

        if (text == "*")
        {
            return new(SmartSplitMode.Overflow, null);
        }

        return new(SmartSplitMode.Good, text);
    }
}

public static class SmartSplitPicker
{
    public static int Pick(IReadOnlyList<SmartSplitPort> ports, string goodId, Func<int, bool> canAccept, ref int cursor)
    {
        var specific = Scan(ports, goodId, canAccept, ref cursor, SmartSplitMode.Good);
        if (specific >= 0)
        {
            return specific;
        }

        var any = Scan(ports, goodId, canAccept, ref cursor, SmartSplitMode.Any);
        if (any >= 0)
        {
            return any;
        }

        return Scan(ports, goodId, canAccept, ref cursor, SmartSplitMode.Overflow);
    }

    static int Scan(IReadOnlyList<SmartSplitPort> ports, string goodId, Func<int, bool> canAccept, ref int cursor, SmartSplitMode mode)
    {
        var count = ports.Count;
        if (count == 0)
        {
            return -1;
        }

        var start = cursor;
        for (var n = 0; n < count; n++)
        {
            var index = (start + n) % count;
            var port = ports[index];
            if (port.Mode != mode || !port.Matches(goodId) || !canAccept(index))
            {
                continue;
            }

            cursor = (index + 1) % count;
            return index;
        }

        return -1;
    }
}
