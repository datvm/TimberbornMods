namespace ConveyorBelt.Algorithm;

public sealed class SimBelt : ISimTarget
{
    readonly List<BeltGood> items = [];
    readonly List<BeltTrail> trails = [];
    bool accepting;
    float tickHours;

    public float ItemsPerHour { get; set; }
    public bool Running { get; set; } = true;

    // A straight, corner, riser, or solid belt. A merger, splitter, or lift lane is not plain.
    public bool Plain { get; init; } = true;
    public Func<string, bool>? CanCarry { get; set; }
    public bool IsStuck { get; private set; }
    public bool Flooded { get; set; }
    public int Generation { get; private set; }
    public ISimLink? Output { get; set; }
    public ISimSource? Input { get; set; }
    public List<BeltGood> Items => items;
    public IReadOnlyList<BeltTrail> Trails => trails;

    public bool CanAccept(string goodId) => Allows(goodId) && BeltMotion.CanAccept(items);

    public bool TryAccept(string goodId, float leftoverHours, float spentHours)
    {
        if (accepting || !Allows(goodId))
        {
            return false;
        }

        if (Flooded)
        {
            return true;
        }

        accepting = true;
        try
        {
            return BeltMotion.TryJoin(
                items,
                goodId,
                leftoverHours,
                ItemsPerHour,
                Generation,
                spentHours,
                PassOn,
                span => Remember(span, spentHours));
        }
        finally
        {
            accepting = false;
        }
    }

    public void BeginTick(int generation)
    {
        Generation = generation;
        trails.Clear();
    }

    public void Move(float hoursPerTick)
    {
        tickHours = hoursPerTick;
        if (!Running)
        {
            BeltMotion.Hold(items);
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                trails.Add(new(item.Id, item.Position, item.Position, 0f, 1f, true));
            }

            IsStuck = false;
            return;
        }

        IsStuck = BeltMotion.Advance(
            items,
            BeltRates.Delta(ItemsPerHour, hoursPerTick),
            Generation,
            TryHandOff,
            span => Remember(span, 0f));
    }

    public void Pull(float hoursPerTick)
    {
        tickHours = hoursPerTick;
        if (!Running || Input is not { } source)
        {
            return;
        }

        var place = BeltTravel.Distance(ItemsPerHour, hoursPerTick);
        var guard = 0;
        while (guard++ < 64 && place >= 0f)
        {
            if (items.Count >= BeltRates.Capacity)
            {
                return;
            }

            if (items.Count > 0)
            {
                var room = items[^1].Position - BeltRates.Spacing;
                if (room < 0f)
                {
                    return;
                }

                if (place > room)
                {
                    place = room;
                }
            }

            if (!source.TryTake(out var id))
            {
                return;
            }

            var before = items.Count;
            if (!TryAccept(id, BeltTravel.Hours(ItemsPerHour, place), 0f))
            {
                return;
            }

            if (items.Count > before)
            {
                place = items[^1].Position - BeltRates.Spacing;
            }
            else
            {
                place -= BeltRates.Spacing;
            }
        }
    }

    public void Clear()
    {
        items.Clear();
        trails.Clear();
        IsStuck = false;
    }

    bool Allows(string goodId)
    {
        if (!Running)
        {
            return false;
        }

        if (CanCarry is not null && !CanCarry(goodId))
        {
            return false;
        }

        return true;
    }

    bool PassOn(string goodId, float leftoverHours, float spentHours)
        => Output is { } next && Deliver(next, goodId, leftoverHours, spentHours);

    bool TryHandOff(string goodId, float overshoot, float from)
    {
        if (Output is not { } connection)
        {
            return false;
        }

        var spent = BeltTravel.Hours(ItemsPerHour, BeltRates.End - from);
        if (spent < 0f)
        {
            spent = 0f;
        }

        var hours = BeltTravel.Hours(ItemsPerHour, overshoot);
        return Deliver(connection, goodId, hours, spent);
    }

    static bool Deliver(ISimLink link, string goodId, float leftoverHours, float spentHours)
    {
        if (!link.TryGetTarget(goodId, out var target) || !target.TryAccept(goodId, leftoverHours, spentHours))
        {
            return false;
        }

        link.Commit();
        return true;
    }

    void Remember(BeltSpan span, float spentHours)
    {
        var distance = span.To - span.From;
        if (distance < 0f)
        {
            distance = 0f;
        }

        var used = BeltTravel.Hours(ItemsPerHour, distance);
        trails.Add(BeltTrail.Slice(span.Id, span.From, span.To, spentHours, used, tickHours, span.Rest));
    }
}
