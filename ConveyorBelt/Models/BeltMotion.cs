namespace ConveyorBelt.Models;

public static class BeltMotion
{
    public static bool CanAccept(IReadOnlyList<BeltGood> items)
    {
        if (items.Count >= BeltRates.Capacity)
        {
            return false;
        }

        if (items.Count == 0)
        {
            return true;
        }

        return items[^1].Position >= BeltRates.Spacing;
    }

    public static bool HeadReady(IReadOnlyList<BeltGood> items)
        => items.Count > 0 && items[0].Position >= BeltRates.End;

    // Where a good can join after this belt has already moved `traveled` this tick.
    public static bool TryArrival(IReadOnlyList<BeltGood> items, float traveled, out float position)
    {
        position = 0f;
        if (items.Count >= BeltRates.Capacity)
        {
            return false;
        }

        var room = items.Count == 0 ? BeltRates.End : items[^1].Position - BeltRates.Spacing;
        if (room < 0f)
        {
            return false;
        }

        position = traveled < room ? traveled : room;
        return true;
    }

    // Moves every good forward by delta. A good at the end leaves when tryLeave accepts it.
    // Returns true when any good wanted to move and could not.
    public static bool Advance(List<BeltGood> items, float delta, Func<string, bool>? tryLeave)
    {
        var stuck = false;
        var limit = BeltRates.End;
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var wanted = item.Position + delta;
            var pos = wanted < limit ? wanted : limit;
            var blocked = wanted > limit + 0.00001f;

            if (i == 0 && pos >= BeltRates.End && tryLeave is not null && tryLeave(item.Id))
            {
                items.RemoveAt(0);
                i--;
                continue;
            }

            if (i == 0 && blocked)
            {
                stuck = true;
            }

            items[i] = item with { Position = pos };
            limit = pos - BeltRates.Spacing;
        }

        return stuck;
    }
}
