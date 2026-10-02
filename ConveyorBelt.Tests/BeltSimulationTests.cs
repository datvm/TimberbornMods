namespace ConveyorBelt.Tests;

public class BeltSimulationTests
{
    const float HoursPerTick = 24f / 768f;

    [Fact]
    public void LogBeltDeliversOneGood()
    {
        List<string> arrived = [];
        var belt = new Lane(1f) { OnLeave = id => arrived.Add(id) };
        belt.Items.Add(new("Log", 0f));

        var ticks = 0;
        var walked = 0f;
        var delta = BeltRates.Delta(1f, HoursPerTick);
        while (walked < BeltRates.End)
        {
            walked += delta;
            ticks++;
        }

        Run(ticks - 1, belt);
        Assert.Empty(arrived);
        Assert.Single(belt.Items);

        Run(1, belt);
        Assert.Equal(["Log"], arrived);
        Assert.Empty(belt.Items);
    }

    [Fact]
    public void FastBeltHandsOffInOneTick()
    {
        var next = new Lane(100f);
        var belt = new Lane(100f) { Next = next };
        belt.Items.Add(new("Log", 0.5f));

        Run(1, next, belt);

        Assert.Empty(belt.Items);
        var landed = Assert.Single(next.Items);
        Assert.Equal("Log", landed.Id);
        Assert.Equal(1f, landed.Position, 3);
    }

    [Fact]
    public void FullBeltStopsTheGood()
    {
        var next = new Lane(100f);
        for (var i = 0; i < BeltRates.Capacity; i++)
        {
            next.Items.Add(new("Log", BeltRates.End - i * BeltRates.Spacing));
        }

        var belt = new Lane(100f) { Next = next };
        belt.Items.Add(new("Plank", 0.5f));

        Run(3, next, belt);

        Assert.Equal("Plank", Assert.Single(belt.Items).Id);
        Assert.Equal(BeltRates.End, belt.Items[0].Position, 3);
        Assert.Equal(BeltRates.Capacity, next.Items.Count);
        Assert.DoesNotContain(next.Items, good => good.Id == "Plank");
    }

    [Fact]
    public void FastBeltPullsSeveralGoods()
    {
        Queue<string> source = new(["Log", "Log", "Log", "Log", "Log"]);
        var belt = new Lane(100f) { Source = () => source.Count == 0 ? null : source.Dequeue() };

        Run(1, belt);

        Assert.Equal(BeltRates.Capacity, belt.Items.Count);
        Assert.Empty(source);
        Assert.Equal(1f, belt.Items[0].Position, 3);
        for (var i = 1; i < belt.Items.Count; i++)
        {
            Assert.Equal(BeltRates.Spacing, belt.Items[i - 1].Position - belt.Items[i].Position, 3);
        }
    }

    [Fact]
    public void EmptyBeltsPassAFastGoodThrough()
    {
        var third = new Lane(100f);
        var second = new Lane(100f) { Next = third };
        var first = new Lane(100f) { Next = second };
        var hours = BeltTravel.Hours(100f, 2.5f);

        Assert.True(first.TryAccept("Log", hours));

        Assert.Empty(first.Items);
        Assert.Empty(second.Items);
        var landed = Assert.Single(third.Items);
        Assert.Equal(0.5f, landed.Position, 3);
    }

    static void Run(int ticks, params Lane[] downstreamFirst)
    {
        var generation = 1;
        for (var tick = 0; tick < ticks; tick++)
        {
            generation++;
            foreach (var lane in downstreamFirst)
            {
                lane.Begin(generation);
            }

            foreach (var lane in downstreamFirst)
            {
                lane.Move(HoursPerTick);
            }

            foreach (var lane in downstreamFirst)
            {
                lane.Pull(HoursPerTick);
            }
        }
    }

    sealed class Lane(float itemsPerHour)
    {
        public readonly List<BeltGood> Items = [];
        public Lane? Next;
        public Func<string?>? Source;
        public Action<string>? OnLeave;
        int generation;
        bool accepting;

        public void Begin(int tickGeneration) => generation = tickGeneration;

        public void Move(float hoursPerTick)
            => BeltMotion.Advance(Items, BeltRates.Delta(itemsPerHour, hoursPerTick), generation, TryHandOff);

        public void Pull(float hoursPerTick)
        {
            if (Source is null)
            {
                return;
            }

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

                var id = Source();
                if (id is null)
                {
                    return;
                }

                var before = Items.Count;
                if (!TryAccept(id, BeltTravel.Hours(itemsPerHour, place)))
                {
                    return;
                }

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

        public bool TryAccept(string goodId, float leftoverHours)
        {
            if (accepting)
            {
                return false;
            }

            accepting = true;
            try
            {
                return BeltMotion.TryJoin(Items, goodId, leftoverHours, itemsPerHour, generation, 0f, (id, left, _) => PassOn(id, left), null);
            }
            finally
            {
                accepting = false;
            }
        }

        bool TryHandOff(string goodId, float overshoot)
        {
            if (Next is not null)
            {
                return Next.TryAccept(goodId, BeltTravel.Hours(itemsPerHour, overshoot));
            }

            if (OnLeave is null)
            {
                return false;
            }

            OnLeave(goodId);
            return true;
        }

        bool PassOn(string goodId, float leftoverHours) => Next is not null && Next.TryAccept(goodId, leftoverHours);
    }
}
