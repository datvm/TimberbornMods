namespace UnstableCoreChallenge.Components;

public record UnstableCoreStabilizerInitializer(UnstableCoreStabilizerStat Stat);

public readonly record struct UnstableCoreStabilizerStat(
    float Days,
    IReadOnlyList<GoodAmount> Payment
)
{
    public string Serialize() => $"{Days}|{string.Join(';', Payment.Select(g => $"{g.GoodId};{g.Amount}"))}";

    public static UnstableCoreStabilizerStat Deserialize(string serialized)
    {
        var parts = serialized.Split('|');
     
        var days = float.Parse(parts[0]);
        
        var paymentParts = parts[1].Split(';');
        var payment = new GoodAmount[paymentParts.Length / 2];
        for (int i = 0; i < paymentParts.Length; i += 2)
        {
            var goodId = paymentParts[i];
            var amount = int.Parse(paymentParts[i + 1]);
            payment[i / 2] = new(goodId, amount);
        }

        return new UnstableCoreStabilizerStat(days, payment);
    }

}