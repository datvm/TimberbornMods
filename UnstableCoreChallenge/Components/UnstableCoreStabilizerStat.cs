namespace UnstableCoreChallenge.Components;

public record UnstableCoreStabilizerInitializer(UnstableCoreStabilizerStat Stat);

public readonly record struct UnstableCoreStabilizerStat(
    float Days,
    IReadOnlyList<GoodAmount> Payment,
    IReadOnlyList<GoodAmount> Rewards
)
{
    public string Serialize() => $"{Days}|" +
        $"{string.Join(';', Payment.Select(g => $"{g.GoodId};{g.Amount}"))}|" +
        $"{string.Join(';', Rewards.Select(g => $"{g.GoodId};{g.Amount}"))}";

    public static UnstableCoreStabilizerStat Deserialize(string serialized)
    {
        var parts = serialized.Split('|');
     
        var days = float.Parse(parts[0]);
        
        var paymentParts = ParseGoodAmounts(parts.Length > 1 ? parts[1] : null);
        var rewardParts = ParseGoodAmounts(parts.Length > 2 ? parts[2] : null);

        return new(days, paymentParts, rewardParts);
    }

    static IReadOnlyList<GoodAmount> ParseGoodAmounts(string? serialized)
    {
        if (string.IsNullOrEmpty(serialized))
        {
            return [];
        }

        var parts = serialized.Split(';');
        var goodAmounts = new GoodAmount[parts.Length / 2];
        for (int i = 0; i < parts.Length; i += 2)
        {
            var goodId = parts[i];
            var amount = int.Parse(parts[i + 1]);
            goodAmounts[i / 2] = new(goodId, amount);
        }

        return goodAmounts;
    }

}