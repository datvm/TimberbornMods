namespace ThePostOffice.Services;

public static class PostOfficeBoxPlan
{
    public const float LayerStep = 0.232f;
    public const int OfficeLayers = 2;
    public const int LodgeLayers = 16;

    const float OfficeGround = 0.02f;
    const float LodgeGround = 0.05f;
    const int LodgeWidth = 4;

    static readonly float[] Offsets = [-0.26f, 0f, 0.26f];

    static PostOfficeBoxPlan()
    {
        List<BoxSlot> offices = [];
        for (var tileX = 0; tileX < 3; tileX++)
        {
            AddCell(offices, tileX, 2, OfficeGround, OfficeLayers);
        }

        List<BoxSlot> lodge = [];
        for (var tileZ = LodgeWidth - 1; tileZ >= 0; tileZ--)
        {
            for (var tileX = 0; tileX < LodgeWidth; tileX++)
            {
                if (tileX == 0 && tileZ == 0)
                {
                    continue;
                }

                AddCell(lodge, tileX, tileZ, LodgeGround, LodgeLayers);
            }
        }

        OfficeSlots = offices;
        LodgeSlots = lodge;
    }

    public static IReadOnlyList<BoxSlot> OfficeSlots { get; }

    public static IReadOnlyList<BoxSlot> LodgeSlots { get; }

    public static int VisibleCount(int totalWeight, int capacity, int slots)
    {
        if (slots <= 0 || capacity <= 0 || totalWeight <= 0)
        {
            return 0;
        }

        if (totalWeight >= capacity)
        {
            return slots;
        }

        return (int)Math.Round(totalWeight / (double)capacity * slots, MidpointRounding.AwayFromZero);
    }

    public static List<BoxShare> FromAmounts(IReadOnlyList<BoxAmount> amounts, int slots)
    {
        var total = 0;
        foreach (var item in amounts)
        {
            if (item.Amount > 0)
            {
                total += item.Amount;
            }
        }

        if (total <= 0 || slots <= 0)
        {
            return [];
        }

        if (total <= slots)
        {
            List<BoxShare> exact = [];
            foreach (var item in amounts)
            {
                if (item.Amount <= 0)
                {
                    continue;
                }

                exact.Add(new(item.Id, item.Amount));
            }

            exact.Sort(ById);
            return exact;
        }

        return Share(amounts, slots);
    }

    public static List<BoxShare> FromCapacity(IReadOnlyList<BoxAmount> weights, int totalWeight, int capacity, int slots)
        => Share(weights, VisibleCount(totalWeight, capacity, slots));

    static List<BoxShare> Share(IReadOnlyList<BoxAmount> goods, int count)
    {
        if (count <= 0)
        {
            return [];
        }

        long total = 0;
        foreach (var item in goods)
        {
            if (item.Amount > 0)
            {
                total += item.Amount;
            }
        }

        if (total <= 0)
        {
            return [];
        }

        List<ShareWork> rows = [];
        var assigned = 0;
        foreach (var item in goods)
        {
            if (item.Amount <= 0)
            {
                continue;
            }

            var exact = item.Amount / (double)total * count;
            var boxes = (int)Math.Floor(exact);
            assigned += boxes;
            rows.Add(new(item.Id, boxes, exact - boxes));
        }

        rows.Sort(static (a, b) =>
        {
            var remainder = b.Remainder.CompareTo(a.Remainder);
            if (remainder != 0)
            {
                return remainder;
            }

            return string.CompareOrdinal(a.Id, b.Id);
        });

        var extra = count - assigned;
        var index = 0;
        while (extra > 0 && rows.Count > 0)
        {
            var row = rows[index];
            rows[index] = row with { Boxes = row.Boxes + 1 };
            extra--;
            index++;
            if (index == rows.Count)
            {
                index = 0;
            }
        }

        List<BoxShare> shares = [];
        foreach (var row in rows)
        {
            if (row.Boxes > 0)
            {
                shares.Add(new(row.Id, row.Boxes));
            }
        }

        shares.Sort(ById);
        return shares;
    }

    static void AddCell(List<BoxSlot> slots, int tileX, int tileZ, float ground, int layers)
    {
        var x = tileX + 0.5f;
        var z = tileZ + 0.5f;
        foreach (var dz in Offsets)
        {
            foreach (var dx in Offsets)
            {
                for (var layer = 0; layer < layers; layer++)
                {
                    slots.Add(new(x + dx, ground + layer * LayerStep, z + dz));
                }
            }
        }
    }

    static int ById(BoxShare left, BoxShare right) => string.CompareOrdinal(left.Id, right.Id);

    readonly record struct ShareWork(string Id, int Boxes, double Remainder);
}

public readonly record struct BoxSlot(float X, float Y, float Z);

public readonly record struct BoxAmount(string Id, int Amount);

public readonly record struct BoxShare(string Id, int Count);
