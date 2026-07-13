namespace UnstableCoreChallenge.Services;

[BindSingleton]
public class UnstableCoreStabilizerService(
    EntityService entityService,
    UnstableCoreSpecService coreSpec,
    GameCycleService gameCycleService,
    NotificationBus notfBus,
    ILoc t
)
{

    public UnstableCoreChallengeStage GetCurrentSpec() => coreSpec.GetStats(gameCycleService.Cycle);

    public UnstableCoreStabilizerInitializer GetChallenge(UnstableCoreChallengeStage spec)
    {
        var days = Random.RandomRangeInt(spec.DaysMin, spec.DaysMax + 1);

        Dictionary<string, int> payments = [];
        var paymentEntries = 0;

        // Science
        if (spec.ScienceChance > 0f && spec.SciencePayment is { } sp)
        {
            var hasScience = spec.ScienceChance >= 1f || Random.Range(0f, 1f) <= spec.ScienceChance;

            if (hasScience)
            {
                var amount = Random.RandomRangeInt(sp.Min, sp.Max + 1);
                AddPaymentAmount(UnstableCoreSpecService.Science, amount);
                paymentEntries++;
            }
        }

        // Other goods
        var entryAmount = Random.RandomRangeInt(spec.PaymentEntries.Min, spec.PaymentEntries.Max + 1) - paymentEntries;
        var tierCount = Math.Min(spec.Payments.Length, coreSpec.GoodTiers.Length);
        for (var tier = tierCount - 1; tier >= 0 && entryAmount > 0; tier--)
        {
            if (spec.Payments[tier] is not { } payment)
            {
                continue;
            }

            var maxCount = Math.Min(payment.MaxCount, entryAmount);
            if (maxCount <= 0)
            {
                continue;
            }

            var minCount = Math.Clamp(payment.MinCount, 0, maxCount);
            var count = Random.RandomRangeInt(minCount, maxCount + 1);
            for (var i = 0; i < count; i++)
            {
                AddPayment(tier, payment);
            }

            entryAmount -= count;
        }

        if (entryAmount > 0 && tierCount > 0 && spec.Payments[0] is { } fallbackPayment)
        {
            for (var i = 0; i < entryAmount; i++)
            {
                AddPayment(0, fallbackPayment);
            }
        }

        return new(new(days, [.. payments.Select(p => new GoodAmount(p.Key, p.Value))]));

        void AddPayment(int tier, ChallengeStagePayment payment)
        {
            var goods = coreSpec.GoodTiers[tier];
            var goodId = goods[Random.RandomRangeInt(0, goods.Length)];
            var amount = Random.RandomRangeInt(payment.MinAmount, payment.MaxAmount + 1);
            AddPaymentAmount(goodId, amount);
        }

        void AddPaymentAmount(string goodId, int amount) 
            => payments[goodId] = payments.GetValueOrDefault(goodId, 0) + amount;
    }

    public void AnnounceNewCore(UnstableCoreStabilizer core) => notfBus.Post(t.T("LV.USC.NewCore"), core);

    public void Delete(UnstableCoreStabilizer stabilizer)
    {
        entityService.Delete(stabilizer);
    }

}
