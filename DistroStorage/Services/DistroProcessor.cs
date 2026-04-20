namespace DistroStorage.Services;

[BindSingleton]
public class DistroProcessor(DistroRegistry registry, IDayNightCycle dayNightCycle) : ITickableSingleton, ILoadableSingleton
{

    float hoursPerTick;

    public void Load()
    {
        hoursPerTick = dayNightCycle.TicksToHours(1);
    }

    public void Tick()
    {
        foreach (var sender in registry.Senders)
        {
            if (!sender.ActiveAndEnabled) { continue; }

            var timer = sender.Timer;
            var newValue = timer.NextTransfer - hoursPerTick;

            if (newValue > 0)
            {
                timer.NextTransfer = newValue;
                continue;
            }

            AttemptTransferringOut(sender);
            timer.Reset();
        }
    }

    void AttemptTransferringOut(IDistroSender sender)
    {
        var goods = sender.Goods
            .Where(g => g.Amount > 0)
            .Select(g => g.GoodId)
            .ToHashSet();

        foreach (var r in sender.GetPrioritizedReceivers())
        {
            var g = r.CanReceiveGood(goods);
            if (g is null) { continue; }

            sender.TransferOut(new(g, 1));
            r.TransferIn(new(g, 1));

            return;
        }
    }

}
