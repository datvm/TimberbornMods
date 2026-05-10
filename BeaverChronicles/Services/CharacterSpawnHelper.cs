namespace BeaverChronicles.Services;

[BindSingleton]
public class CharacterSpawnHelper(
    FindEntityHelper spawnLocationHelper,
    BeaverFactory beaverFactory,
    BotFactory botFactory
)
{

    public Bot SpawnBot(Vector3 position) => botFactory.Create(position, Quaternion.identity);
    public Beaver SpawnAdult(Vector3 position) => beaverFactory.CreateNewbornAdult(position);
    public Beaver SpawnChild(Vector3 position) => beaverFactory.CreateNewbornChild(position);

    public IReadOnlyList<BaseComponent> Spawn(int count, Vector3 position, CharacterType characterType)
    {
        List<BaseComponent> result = [];

        for (int i = 0; i < count; i++)
        {
            result.Add(Spawn(position, characterType));
        }

        return result;
    }

    public BaseComponent Spawn(Vector3 position, CharacterType characterType) => characterType switch
    {
        CharacterType.Bot => SpawnBot(position),
        CharacterType.AdultBeaver => SpawnAdult(position),
        CharacterType.ChildBeaver => SpawnChild(position),
        _ => throw new ArgumentOutOfRangeException(nameof(characterType), characterType, null),
    };

    public bool FindAnySpawnSpot(out Vector3 result, DistrictCenter? preferredDistrict = null)
    {
        foreach (var comp in spawnLocationHelper.FindEntityToSpawnNearby(preferredDistrict))
        {
            if (comp is Beaver || comp is Bot)
            {
                result = comp.Transform.position;
                return true;
            }

            var accessible = comp.GetComponent<BuildingAccessible>();
            if (accessible)
            {
                var unblocked = accessible.Accessible.UnblockedSingleAccess;

                if (unblocked.HasValue)
                {
                    result = unblocked.Value;
                    return true;
                }
            }

            if (comp is BlockObject bo)
            {
                result = CoordinateSystem.GridToWorldCentered(bo.Coordinates);
                return true;
            }
        }

        result = default;
        return false;
    }

}
