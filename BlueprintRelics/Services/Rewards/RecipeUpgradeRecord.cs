namespace BlueprintRelics.Services.Rewards;

public class RecipeUpgradeRecord(string id)
{
    public static readonly RecipeUpgradeRecordSerializer Serializer = new();

    public string Id { get; } = id;
    public int CapacityUpgrades { get; internal set; }
    public int TimeReductionUpgrades { get; internal set; }
    public Dictionary<string, int> OutputUpgrades { get; } = [];

    public class RecipeUpgradeRecordSerializer : IValueSerializer<RecipeUpgradeRecord>
    {
        static readonly PropertyKey<string> IdKey = new("Id");
        static readonly PropertyKey<int> CapacityUpgradesKey = new("CapacityUpgrades");
        static readonly PropertyKey<int> TimeReductionUpgradesKey = new("TimeReductionUpgrades");
        static readonly ListKey<string> OutputUpgradesKey = new("OutputUpgrades");

        public Obsoletable<RecipeUpgradeRecord> Deserialize(IValueLoader valueLoader)
        {
            var obj = valueLoader.AsObject();

            var result = new RecipeUpgradeRecord(obj.Get(IdKey))
            {
                CapacityUpgrades = obj.Get(CapacityUpgradesKey),
                TimeReductionUpgrades = obj.Get(TimeReductionUpgradesKey),
            };

            result.OutputUpgrades.AddRange(obj.GetPairs<string, int>(OutputUpgradesKey));

            return result;
        }

        public void Serialize(RecipeUpgradeRecord value, IValueSaver valueSaver)
        {
            var obj = valueSaver.AsObject();

            obj.Set(IdKey, value.Id);
            obj.Set(CapacityUpgradesKey, value.CapacityUpgrades);
            obj.Set(TimeReductionUpgradesKey, value.TimeReductionUpgrades);
            obj.SetPairs(OutputUpgradesKey, value.OutputUpgrades);
        }
    }

}