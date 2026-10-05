namespace ConveyorBelt.Tests;

// The shot: three full belts leave the building and meet the junctions on the right.
// Those junctions feed one fast loop. The loop is empty except one good on the far side.
public class BuildingLineTests
{
    const float Tick = 24f / 768f;
    const float Slow = 10f;
    const float Fast = 100f;

    [Fact]
    public void ThreeFullBeltsFeedAnEmptyFastLoop()
    {
        var top = new Lane(Slow);
        var middle = new Lane(Slow);
        var bottom = new Lane(Slow);
        Fill(top, "Top");
        Fill(middle, "Mid");
        Fill(bottom, "Bot");

        var east = new Lane(Fast);
        var south = new Lane(Fast);
        var west = new Lane(Fast);
        var north = new Lane(Fast);
        east.Next = south;
        south.Next = west;
        west.Next = north;
        north.Next = east;
        west.Items.Add(new("Far", 0.5f));

        var joined = new Lane(BeltRates.Junction);
        top.Next = joined;
        middle.Next = joined;
        bottom.Next = joined;
        joined.Next = east;

        Lane[] feeders = [top, middle, bottom];
        Lane[] loop = [east, south, west, north];
        var generation = 1;
        for (var tick = 0; tick < 8; tick++)
        {
            generation++;
            foreach (var lane in feeders)
            {
                lane.Begin(generation);
            }

            foreach (var lane in loop)
            {
                lane.Begin(generation);
            }

            joined.Begin(generation);

            var first = tick % feeders.Length;
            for (var i = 0; i < feeders.Length; i++)
            {
                feeders[(first + i) % feeders.Length].Move(Tick);
            }

            joined.Move(Tick);
            foreach (var lane in loop)
            {
                lane.Move(Tick);
            }

            foreach (var lane in feeders)
            {
                lane.Pull(Tick);
            }
        }

        List<BeltGood> riding = [..east.Items, ..south.Items, ..west.Items, ..north.Items];
        Assert.Contains(riding, good => good.Id == "Top");
        Assert.Contains(riding, good => good.Id == "Mid");
        Assert.Contains(riding, good => good.Id == "Bot");
    }

    static void Fill(Lane lane, string id)
    {
        for (var i = 0; i < BeltRates.Capacity; i++)
        {
            lane.Items.Add(new(id, BeltRates.End - i * BeltRates.Spacing));
        }
    }

    interface Door
    {
        bool Open { get; }

        bool TryAccept(string id, float leftover, float spent);
    }

    sealed class Lane(float itemsPerHour) : Door
    {
        public readonly List<BeltGood> Items = [];
        public Lane? Next;
        int generation;
        bool accepting;
        int supplied;

        public bool Open => BeltMotion.CanAccept(Items);

        public void Begin(int tickGeneration) => generation = tickGeneration;

        public void Move(float hoursPerTick)
            => BeltMotion.Advance(Items, BeltRates.Delta(itemsPerHour, hoursPerTick), generation, TryHandOff, null);

        public void Pull(float hoursPerTick)
        {
            var place = BeltTravel.Distance(itemsPerHour, hoursPerTick);
            var guard = 0;
            while (guard++ < 64 && place >= 0f)
            {
                if (Items.Count >= BeltRates.Capacity)
                {
                    return;
                }

                if (Items.Count > 0)
                {
                    var room = Items[^1].Position - BeltRates.Spacing;
                    if (room < 0f)
                    {
                        return;
                    }

                    if (place > room)
                    {
                        place = room;
                    }
                }

                var before = Items.Count;
                if (!TryAccept($"Supply{supplied}", BeltTravel.Hours(itemsPerHour, place), 0f))
                {
                    return;
                }

                supplied++;
                if (Items.Count > before)
                {
                    place = Items[^1].Position - BeltRates.Spacing;
                }
                else
                {
                    place -= BeltRates.Spacing;
                }
            }
        }

        public bool TryAccept(string id, float leftover, float spent)
        {
            if (accepting)
            {
                return false;
            }

            accepting = true;
            try
            {
                return BeltMotion.TryJoin(
                    Items,
                    id,
                    leftover,
                    itemsPerHour,
                    generation,
                    spent,
                    PassOn,
                    null);
            }
            finally
            {
                accepting = false;
            }
        }

        bool TryHandOff(string id, float overshoot, float from)
        {
            var door = (Door?)Next;
            if (door is null || !door.Open)
            {
                return false;
            }

            var spent = BeltTravel.Hours(itemsPerHour, BeltRates.End - from);
            if (spent < 0f)
            {
                spent = 0f;
            }

            return door.TryAccept(id, BeltTravel.Hours(itemsPerHour, overshoot), spent);
        }

        bool PassOn(string id, float leftover, float spent) => Next is not null && Next.TryAccept(id, leftover, spent);
    }
}
