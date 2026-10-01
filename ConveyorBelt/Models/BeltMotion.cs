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

    public static bool Advance(List<BeltGood> items, float delta, Func<string, bool>? tryLeave)
        => Advance(items, delta, 0, tryLeave is null ? null : (id, _) => tryLeave(id));

    // Moves every good forward by delta. A good at the end leaves when tryLeave accepts it.
    // overshoot is how far past the end the good still wanted to travel, in belt lengths.
    // Goods stamped with generation already sit at their final position for this tick.
    // Returns true when any good wanted to move and could not.
    public static bool Advance(List<BeltGood> items, float delta, int generation, Func<string, float, bool>? tryLeave)
    {
        var stuck = false;
        var limit = BeltRates.End;
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (generation != 0 && item.Generation == generation)
            {
                limit = item.Position - BeltRates.Spacing;
                continue;
            }

            var wanted = item.Position + delta;
            var pos = wanted < limit ? wanted : limit;
            var blocked = wanted > limit + 0.00001f;
            var overshoot = wanted - BeltRates.End;

            if (i == 0 && pos >= BeltRates.End && tryLeave is not null && tryLeave(item.Id, overshoot > 0f ? overshoot : 0f))
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
