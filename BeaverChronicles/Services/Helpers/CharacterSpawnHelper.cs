namespace BeaverChronicles.Services.Helpers;

[BindSingleton]
public class CharacterSpawnHelper(
    FindEntityHelper spawnLocationHelper,
    BeaverFactory beaverFactory,
    BotFactory botFactory
)
{

    public void SpawnBot(Vector3 position) => botFactory.Create(position);
    public void SpawnAdult(Vector3 position) => beaverFactory.CreateAdult(position, 0f);
    public void SpawnChild(Vector3 position) => beaverFactory.CreateChild(position, 0f);

    public void Spawn(int count, Vector3 position, CharacterType characterType)
    {
        for (int i = 0; i < count; i++)
        {
            Spawn(position, characterType);
        }
    }

    public void Spawn(Vector3 position, CharacterType characterType)
    {
        switch (characterType)
        {
            case CharacterType.Bot:
                SpawnBot(position);
                break;
            case CharacterType.AdultBeaver:
                SpawnAdult(position);
                break;
            case CharacterType.ChildBeaver:
                SpawnChild(position);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(characterType), characterType, null);
        }
    }

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
