namespace EarthquakeWeather.Components;

public class EarthquakeStockpileComponent : BaseComponent, IFinishedStateListener, IEntityEffectDescriber
{
    public const string ExplosivesId = "Explosives";

    const float GoodLostMin = .25f;
    const float GoodLostMax = .5f;

#nullable disable
    Stockpile stockpile;
    EarthquakeRegistry earthquakeRegistry;
#nullable enable

    public bool HasStockpileReinforcement { get; private set; }

    [Inject]
    public void Inject(EarthquakeRegistry earthquakeRegistry)
    {
        this.earthquakeRegistry = earthquakeRegistry;
    }

    public void Start()
    {
        stockpile = GetComponentFast<Stockpile>();
        if (!stockpile) { return; }

        var reno = this.GetRenovationComponent();
        HasStockpileReinforcement = reno.HasRenovation(EqPileReinforcedProvider.RenoId);
        if (!HasStockpileReinforcement)
        {
            reno.RenovationCompleted += OnRenoCompleted;

            var eq = this.GetEarthquakeComponent();
            eq.OnAfterEarthquakeDamage += OnAfterEqDamage;
        }
    }

    void OnAfterEqDamage(object sender, EarthquakeBuildingAfterDamageEventArgs e)
    {
        if (HasStockpileReinforcement) { return; }

        var inventory = stockpile.Inventory;
        var stock = inventory.UnreservedStock().ToArray();
        
        var dynamiteStock = stock.FirstOrDefault(i => i.GoodId == ExplosivesId);
        var hasDynamite = dynamiteStock.Amount > 0;

        // Remove stock before destroying so they don't recover it
        var inventoryLost = hasDynamite ? 1 : UnityEngine.Random.Range(GoodLostMin, GoodLostMax);

        foreach (var item in stock)
        {
            var amount = Mathf.FloorToInt(item.Amount * inventoryLost);
            if (amount > 0)
            {
                inventory.Take(new(item.GoodId, amount));
                e.Notifications.LogLostItem(item.GoodId, amount);
            }
        }

        if (hasDynamite)
        {
            var hp = this.GetHPComponent();
            hp.Damage(hp.Durability);
            e.Notifications.LogDetonatedExplosiveStorage();
        }
    }

    private void OnRenoCompleted(BuildingRenovation obj)
    {
        if (obj.Id != EqPileReinforcedProvider.RenoId) { return; }

        HasStockpileReinforcement = true;

        var eq = this.GetEarthquakeComponent();
        eq.OnAfterEarthquakeDamage -= OnAfterEqDamage;
    }

    public void OnEnterFinishedState()
    {
        if (!stockpile) { return; }
        earthquakeRegistry.Register(this);
    }

    public void OnExitFinishedState()
    {
        if (!stockpile) { return; }
        earthquakeRegistry.Unregister(this);
    }

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle)
        => HasStockpileReinforcement
            ? new(t.T("LV.EQ.EqPileReinforced"), t.T("LV.EQ.EqPileReinforcedDesc"))
            : null;
}
