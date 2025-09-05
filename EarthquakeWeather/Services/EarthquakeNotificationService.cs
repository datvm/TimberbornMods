namespace EarthquakeWeather.Services;

public class EarthquakeNotificationService(
    ILoc t,
    QuickNotificationService quickNotificationService,
    IGoodService goods
)
{
    Vector2Int location;
    readonly List<string> notifications = [];
    public int DamagedBuildings { get; private set; }
    public int DetonatedExplosives { get; private set; }
    public int DetonatedExplosiveStorages { get; private set; }
    public int BeaversInjured { get; private set; }
    public int BotsDamaged { get; private set; }
    public Dictionary<string, int> LostItems { get; private set; } = [];

    public void Clear()
    {
        notifications.Clear();
        DamagedBuildings = 0;
        DetonatedExplosives = 0;
        DetonatedExplosiveStorages = 0;
        LostItems.Clear();
    }

    public void AppendNotification(string message)
    {
        notifications.Add(message);
    }

    public void SetLocation(Vector2Int location) => this.location = location;

    public void ShowNotification()
    {
        if (BeaversInjured > 0)
        {
            notifications.Add(t.T("LV.EQ.BeaversInjuredNotf", BeaversInjured));
        }

        if (BotsDamaged > 0)
        {
            notifications.Add(t.T("LV.EQ.BotsDamagedNotf", BotsDamaged));
        }

        if (DamagedBuildings > 0)
        {
            notifications.Add(t.T("LV.EQ.BuildingDamageNotf", DamagedBuildings));
        }

        if (DetonatedExplosives > 0)
        {
            notifications.Add(t.T("LV.EQ.ExplosivesNotf", DetonatedExplosives));
        }

        if (DetonatedExplosiveStorages > 0)
        {
            notifications.Add(t.T("LV.EQ.ExplosiveStorageNotf", DetonatedExplosiveStorages));
        }

        if (LostItems.Count > 0)
        {
            notifications.Add(t.T("LV.EQ.ItemsLostNotf"));
            foreach (var (itemId, count) in LostItems)
            {
                notifications.Add("  " + t.T("LV.EQ.ItemLostNotf", count, goods.GetGood(itemId).PluralDisplayName.Value));
            }
        }

        var content = t.T("LV.EQ.ResultNotification",
            location.x,
            location.y,
            string.Join(Environment.NewLine, notifications));

        quickNotificationService.SendWarningNotification(content);
        ModdableWeatherUtils.Log(() => content);

        Clear();
    }

    public void LogDetonatedExplosive() => DetonatedExplosives++;
    public void LogDamagedBuilding() => DamagedBuildings++;
    public void LogDetonatedExplosiveStorage() => DetonatedExplosiveStorages++;
    public void LogInjuredBeaver() => BeaversInjured++;
    public void LogDamagedBot() => BotsDamaged++;

    public void LogLostItem(string itemId, int count)
        => LostItems[itemId] = LostItems.TryGetValue(itemId, out var existing) ? existing + count : count;

}
