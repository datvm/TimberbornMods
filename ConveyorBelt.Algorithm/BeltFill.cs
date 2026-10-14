namespace ConveyorBelt.Algorithm;

public static class BeltFill
{
    public const string LogId = "Log";

    public static void AddLogs(List<BeltGood> items, Func<string, bool>? canCarry = null)
        => Add(items, LogId, canCarry);

    public static void Add(List<BeltGood> items, string id, Func<string, bool>? canCarry)
    {
        if (canCarry is not null && !canCarry(id))
        {
            return;
        }

        var back = items.Count == 0 ? BeltRates.End + BeltRates.Spacing : items[^1].Position;
        while (items.Count < BeltRates.Capacity)
        {
            var position = back - BeltRates.Spacing;
            if (position < 0f)
            {
                return;
            }

            items.Add(new(id, position, Previous: position));
            back = position;
        }
    }
}
