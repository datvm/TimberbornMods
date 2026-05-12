namespace BeaverChronicles.Models;

public readonly record struct OnNewChroniclePaymentEvent();

public class ActiveEventPayment
{
    public const string ScienceId = "Science";

    public ActiveEventPaymentItem? Science { get; set; }
    public ActiveEventPaymentItem[] Goods { get; set; } = [];

    [JsonIgnore]
    public bool HasScience => Science != null;

    [JsonIgnore]
    public bool HasGoods => Goods.Length > 0;

    [JsonIgnore]
    public bool Paid => (Science is null || Science.IsPaid) && Goods.FastAll(g => g.IsPaid);

    public bool NeedToPay(string id, out int remainingAmount)
    {
        var p = id == ScienceId ? Science : Goods.FirstOrDefault(g => g.Id == id);
        if (p is null || p.IsPaid)
        {
            remainingAmount = 0;
            return false;
        }

        remainingAmount = p.RemainingAmount;
        return true;
    }

    public bool NeedToPayScience(out int remainingAmount) => NeedToPay(ScienceId, out remainingAmount);

    public void Pay(string id, int amount)
    {
        var p = (id == ScienceId ? Science : Goods.FirstOrDefault(g => g.Id == id))
            ?? throw new ArgumentException($"No payment item found with id '{id}'.");
        
        p.Pay(amount);
    }

    public void PayScience(int amount) => Pay(ScienceId, amount);

    public string Serialize() => JsonConvert.SerializeObject(this);
    public static ActiveEventPayment Deserialize(string json) => JsonConvert.DeserializeObject<ActiveEventPayment>(json)
        ?? throw new ArgumentNullException(nameof(json));

}

public class ActiveEventPaymentItem(string id, int amount)
{
    public string Id => id;
    public int Amount => amount;
    public int PaidAmount { get; set; }

    [JsonIgnore]
    public int RemainingAmount => amount - PaidAmount;

    internal void Pay(int amount)
    {
        if (amount > RemainingAmount)
        {
            throw new InvalidOperationException($"Cannot pay more than the remaining amount. Remaining: {RemainingAmount}, attempted payment: {amount}");
        }

        PaySafe(amount);
    }

    internal void PaySafe(int amount)
    {
        PaidAmount = Math.Min(PaidAmount + amount, Amount);
    }

    [JsonIgnore]
    public bool IsPaid => PaidAmount >= Amount;

    [JsonIgnore]
    public bool IsScience => Id == ActiveEventPayment.ScienceId;
}