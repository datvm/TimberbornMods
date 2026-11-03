namespace ThePostOffice.Models;

static class PostOfficeSave
{
    public static string WriteGoods(IReadOnlyList<GoodAmount> goods)
    {
        List<string> parts = [];
        foreach (var good in goods)
        {
            parts.Add(good.GoodId);
            parts.Add(good.Amount.ToString());
        }

        return string.Join("|", parts);
    }

    public static string WriteParcel(PostOfficeParcel parcel)
    {
        var body = WriteGoods(parcel.Goods);
        return body.Length == 0
            ? parcel.Auto ? "1" : "0"
            : string.Join("|", parcel.Auto ? "1" : "0", body);
    }

    public static string WriteRule(PostOfficeAutoRule rule)
        => string.Join("|", rule.GoodId, rule.Target, rule.Margin);

    public static ImmutableArray<GoodAmount> ReadGoods(string line)
    {
        if (line.Length == 0)
        {
            return [];
        }

        var parts = line.Split('|');
        if (parts.Length < 2 || parts.Length % 2 != 0)
        {
            return [];
        }

        List<GoodAmount> goods = [];
        for (var i = 0; i < parts.Length; i += 2)
        {
            if (!int.TryParse(parts[i + 1], out var amount) || amount <= 0 || parts[i].Length == 0)
            {
                continue;
            }

            goods.Add(new(parts[i], amount));
        }

        return [..goods];
    }

    public static PostOfficeParcel? ReadParcel(string line)
    {
        var split = line.IndexOf('|');
        if (split <= 0)
        {
            return null;
        }

        var goods = ReadGoods(line[(split + 1)..]);
        if (goods.Length == 0)
        {
            return null;
        }

        return new(goods, line[..split] == "1");
    }

    public static PostOfficeAutoRule? ReadRule(string line)
    {
        var parts = line.Split('|');
        if (parts.Length != 3 || parts[0].Length == 0)
        {
            return null;
        }

        if (!int.TryParse(parts[1], out var target) || target <= 0)
        {
            return null;
        }

        if (!int.TryParse(parts[2], out var margin) || margin < 0)
        {
            return null;
        }

        return new(parts[0], target, margin);
    }
}
