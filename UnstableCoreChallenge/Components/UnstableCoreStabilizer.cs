namespace UnstableCoreChallenge.Components;

[AddTemplateModule2(typeof(UnstableCore))]
public class UnstableCoreStabilizer(
    UnstableCoreStabilizerService service
) : BaseComponent, IPersistentEntity, IAwakableComponent, IInitializableEntity, IDeletableEntity
{
    static readonly ComponentKey SaveKey = new(nameof(UnstableCoreStabilizer));
    static readonly PropertyKey<string> StatsKey = new("Stats");
    static readonly PropertyKey<string> GoodsPaidKey = new("GoodsPaid");

    UnstableCoreStabilizerStat stat;
    readonly Dictionary<string, int> goodsPaid = [];
    StatusToggle coreStabilizedStatus = null!;

    public bool IsFinished => GetCurrentPayments().All(p => p.Finished);

    int? sciencePayment;
    public int SciencePayment
        => sciencePayment ??= stat.Payment.FirstOrDefault(p => p.GoodId == UnstableCoreSpecService.Science).Amount;

    bool? sciencePaid;
    public bool SciencePaid
    {
        get => sciencePaid ??= GetCurrentPayments().FirstOrDefault(p => p.IsScience).Finished;
        set => sciencePaid = value;
    }

#nullable disable
    TimedComponentActivator timedComponentActivator;
#nullable enable

    public void Awake()
    {
        timedComponentActivator = GetComponent<TimedComponentActivator>();
        timedComponentActivator.Activated += OnTimedActivatorActivated;

        var t = service.t;
        coreStabilizedStatus = StatusToggle.CreateNormalStatusWithAlertAndFloatingIcon(
            "CoreStabilized", t.T("LV.USC.CoreStabilized"), t.T("LV.USC.CoreStabilizedShort"));
    }

    public IEnumerable<StabilizerPayment> GetCurrentPayments()
    {
        foreach (var amount in stat.Payment)
        {
            yield return new(amount.GoodId, amount.Amount, goodsPaid.GetOrDefault(amount.GoodId));
        }
    }

    public void InitializeEntity()
    {
        GetComponent<StatusSubject>().RegisterStatus(coreStabilizedStatus);

        if (stat != default)
        {
            if (IsFinished)
            {
                Stabilize();
            }

            return;
        }

        if (GetComponent<UnstableCoreStabilizerInitializer>() is { } initializer)
        {
            InitializeNewChallenge(initializer.Stat);
        }
        else // Not from  the challenge
        {
            StabilizeAndDestroy();
        }
    }

    public void Pay(string goodId, int amount)
    {
        var total = goodsPaid[goodId] = goodsPaid.GetOrDefault(goodId) + amount;

        if (total >= stat.Payment.FirstOrDefault(p => p.GoodId == goodId).Amount)
        {
            if (goodId == UnstableCoreSpecService.Science)
            {
                SciencePaid = true;
            }

            if (IsFinished)
            {
                Stabilize();
            }
        }
    }

    public void DebugPayAll()
    {
        foreach (var payment in stat.Payment)
        {
            goodsPaid[payment.GoodId] = payment.Amount;
        }

        Pay(stat.Payment[0].GoodId, 0); // Trigger the check for stabilization
    }

    void InitializeNewChallenge(UnstableCoreStabilizerStat stat)
    {
        this.stat = stat;

        timedComponentActivator.DaysUntilActivation = stat.Days;
        timedComponentActivator.CyclesUntilCountdownActivation = 0;
        timedComponentActivator.ActivateIfItsTime();

        GetComponent<UnstableCoreVisualisation>().OnUnselect();
        service.AnnounceNewCore(this);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }
        stat = UnstableCoreStabilizerStat.Deserialize(s.Get(StatsKey));

        if (s.Has(GoodsPaidKey))
        {
            var goodsPaidString = s.Get(GoodsPaidKey);
            var parts = goodsPaidString.Split(';');
            for (int i = 0; i < parts.Length; i += 2)
            {
                var goodId = parts[i];
                var amount = int.Parse(parts[i + 1]);
                goodsPaid[goodId] = amount;
            }
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(StatsKey, stat.Serialize());

        if (goodsPaid.Count > 0)
        {
            s.Set(GoodsPaidKey, string.Join(';', goodsPaid.Select(kv => $"{kv.Key};{kv.Value}")));
        }
    }

    public void Stabilize()
    {
        GetComponent<UnstableCoreExplosionBlocker>().BlockExplosion();
        GetComponent<ActivationWarningStatus>()._statusToggle.Deactivate();
        coreStabilizedStatus.Activate();
    }

    public void StabilizeAndDestroy()
    {
        Stabilize();
        service.Delete(this);
    }

    void OnTimedActivatorActivated(object sender, EventArgs e)
    {
        if (IsFinished)
        {
            service.Delete(this);
        }
    }

    public void DeleteEntity()
    {
        if (!IsFinished || !(stat.Rewards?.Count > 0)) { return; }

        var coords = GetComponent<BlockObject>().Coordinates;
        service.SpawnRewards(coords, stat.Rewards);
    }
}

public readonly record struct StabilizerPayment(string GoodId, int Amount, int Paid)
{
    public bool IsScience => GoodId == UnstableCoreSpecService.Science;

    public bool Finished => Paid >= Amount;
}
