namespace BrainPowerSPs.Services;

public class PowerSPPrefabModifier(
    ScientificProjectUnlockRegistry unlocks,
    ScientificProjectRegistry registry
) : IPrefabModifier, ILoadableSingleton
{
    bool hasBotUpgrade;
    bool hasWorkerUpgrade;
    bool hasWindmillUpgrade;

    static readonly FrozenSet<string> WindmillPrefabs = ["Windmill.Folktails", "LargeWindmill.Folktails"];

    public void Load()
    {
        hasBotUpgrade = unlocks.Contains(PowerProjectsUtils.PowerWheelBotUpId);
        hasWorkerUpgrade = unlocks.Contains(PowerProjectsUtils.PowerWheelWorkerUpId);
        hasWindmillUpgrade = unlocks.Contains(PowerProjectsUtils.WindmillSizeUpId);
    }

    public GameObject? Modify(GameObject prefab, PrefabSpec prefabSpec, GameObject original) 
        => WindmillPrefabs.Contains(prefabSpec.PrefabName) ? ModifyWindmills(prefab) : ModifyPowerWheel(prefab);

    GameObject ModifyWindmills(GameObject prefab)
    {
        var blockObj = prefab.GetComponent<BlockObjectSpec>();

        var spec = blockObj._blocksSpec._blockSpecs;
        var size = blockObj._blocksSpec._size;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (x == 1 && y == 1) { continue; } // Keep the middle piece only

                for (int z = 0; z < size.z; z++)
                {
                    var i = x + size.x * (y + size.y * z);
                    spec[i] = spec[i] with
                    {
                        _matterBelow = MatterBelow.Any,
                        _occupations = BlockOccupations.None,
                        _stackable = BlockStackable.None,
                        _useNewOccupation = true,
                    };
                }
            }
        }

        return prefab;
    }

    GameObject ModifyPowerWheel(GameObject prefab)
    {
        var workplace = prefab.GetComponent<WorkplaceSpec>();
        if (hasBotUpgrade)
        {
            workplace._disallowOtherWorkerTypes = false;
        }

        if (hasWorkerUpgrade)
        {
            var spec = registry.GetProject(PowerProjectsUtils.PowerWheelWorkerUpId);
            var reducing = (int)spec.Parameters[0];

            if (workplace._maxWorkers > reducing)
            {
                workplace._defaultWorkers = workplace._maxWorkers = workplace._maxWorkers - reducing;
            }
        }

        return prefab;
    }

    public bool ShouldModify(string prefabName, PrefabSpec prefabSpec)
        => (prefabName == "PowerWheel.Folktails" && hasBotUpgrade)
        || (prefabName == "LargePowerWheel.IronTeeth" && (hasWorkerUpgrade || hasBotUpgrade))
        || (hasWindmillUpgrade && WindmillPrefabs.Contains(prefabName));
}
