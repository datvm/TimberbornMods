namespace ConveyorBelt.Algorithm;

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
        => Advance(items, delta, 0, tryLeave is null ? null : (id, _, _) => tryLeave(id), null);

    public static bool Advance(List<BeltGood> items, float delta, int generation, Func<string, float, bool>? tryLeave)
        => Advance(items, delta, generation, tryLeave is null ? null : (id, over, _) => tryLeave(id, over), null);

    // Places a good that still has leftoverHours of travel. An empty belt that the good can
    // finish passes it on instead of keeping it. Otherwise it stops where this belt's speed
    // spends the leftover time.
    public static bool TryJoin(
        List<BeltGood> items,
        string goodId,
        float leftoverHours,
        float itemsPerHour,
        int generation,
        float spentHours,
        Func<string, float, float, bool>? passOn,
        Action<BeltSpan>? note)
    {
        var distance = BeltTravel.Distance(itemsPerHour, leftoverHours);
        if (items.Count == 0 && distance >= BeltRates.End && passOn is not null)
        {
            var cross = BeltTravel.Hours(itemsPerHour, BeltRates.End);
            var left = leftoverHours - cross;
            if (left < 0f)
            {
                left = 0f;
            }

            if (passOn(goodId, left, spentHours + cross))
            {
                note?.Invoke(new(goodId, 0f, BeltRates.End, false));
                return true;
            }
        }

        if (!TryArrival(items, distance, out var position))
        {
            return false;
        }

        items.Add(new(goodId, position, generation));
        note?.Invoke(new(goodId, 0f, position, true));
        return true;
    }

    public static void Hold(List<BeltGood> items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item.Previous == item.Position)
            {
                continue;
            }

            items[i] = item with { Previous = item.Position };
        }
    }

    // Moves every good forward by delta. A good at the end leaves when tryLeave accepts it.
    // overshoot is how far past the end the good still wanted to travel, in belt lengths.
    // Goods stamped with generation already sit at their final position for this tick.
    // Returns true when any good wanted to move and could not.
    public static bool Advance(
        List<BeltGood> items,
        float delta,
        int generation,
        Func<string, float, float, bool>? tryLeave,
        Action<BeltSpan>? note)
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

            if (i == 0 && pos >= BeltRates.End && tryLeave is not null && tryLeave(item.Id, overshoot > 0f ? overshoot : 0f, item.Position))
            {
                note?.Invoke(new(item.Id, item.Position, BeltRates.End, false));
                items.RemoveAt(0);
                i--;
                continue;
            }

            if (i == 0 && blocked)
            {
                stuck = true;
            }

            note?.Invoke(new(item.Id, item.Position, pos, true));
            items[i] = item with { Previous = item.Position, Position = pos };
            limit = pos - BeltRates.Spacing;
        }

        return stuck;
    }
}
