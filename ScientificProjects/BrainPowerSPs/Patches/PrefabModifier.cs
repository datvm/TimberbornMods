namespace BrainPowerSPs.Patches;

public class PrefabModifier(ScientificProjectService projects) : IPrefabGroupServiceFrontRunner
{
    static readonly ConditionalWeakTable<GameObject, object> modifiedPrefabs = [];

    const string LargePowerWheelName = "LargePowerWheel.IronTeeth";
    static readonly ImmutableHashSet<string> BotBuildings = [LargePowerWheelName, "PowerWheel.Folktails"];

    const string WindmillName = "Windmill.Folktails";
    const string LargeWindmillName = "LargeWindmill.Folktails";

    void ModifyPowerWheels(GameObject prefab, PrefabSpec prefabSpec, ScientificProjectInfo botProject, ScientificProjectInfo workerProject)
    {
        if (!BotBuildings.Contains(prefabSpec.PrefabName)) { return; }

        var workplace = prefabSpec.GetComponentFast<WorkplaceSpec>();
        if (!workplace)
        {
            Debug.LogWarning($"Prefab {prefabSpec.PrefabName} does not have a {nameof(WorkplaceSpec)} component.");
            return;
        }

        workplace._disallowOtherWorkerTypes = !botProject.Unlocked;

        if (prefabSpec.PrefabName == LargePowerWheelName)
        {
            var hasOriginalValue = modifiedPrefabs.TryGetValue(prefab, out var originalValues);
            if (!hasOriginalValue)
            {
                originalValues = new Tuple<int, int>(workplace._maxWorkers, workplace._defaultWorkers);
            }

            var (originalMaxWorker, originalDefaultWorker) = (Tuple<int, int>)originalValues;

            var expectedMaxWorkers = originalMaxWorker -
                (workerProject.Unlocked ? Mathf.RoundToInt(workerProject.Spec.Parameters[0]) : 0);
            workplace._maxWorkers = expectedMaxWorkers;

            // Reset to default first
            workplace._defaultWorkers = originalDefaultWorker;

            // Then set to max if needed
            if (workplace._defaultWorkers > expectedMaxWorkers)
            {
                workplace._defaultWorkers = expectedMaxWorkers;
            }

            if (!hasOriginalValue)
            {
                modifiedPrefabs.Add(prefab, originalValues);
            }
        }
    }

    void ModifyWindmills(GameObject prefab, PrefabSpec prefabSpec, ScientificProjectInfo windmillProject)
    {
        if (prefabSpec.PrefabName != WindmillName && prefabSpec.PrefabName != LargeWindmillName) { return; }

        var blockObjSpec = prefabSpec.GetComponentFast<BlockObjectSpec>();
        if (!blockObjSpec)
        {
            Debug.LogWarning($"Prefab {prefabSpec.PrefabName} does not have a {nameof(BlockObjectSpec)} component.");
            return;
        }

        var hasOriginalSpec = modifiedPrefabs.TryGetValue(prefab, out var originalSpecObj);
        var originalSpec = hasOriginalSpec ? (BlocksSpec)originalSpecObj : blockObjSpec._blocksSpec;
        if (!hasOriginalSpec)
        {
            modifiedPrefabs.Add(prefab, originalSpec);
        }

        if (windmillProject.Unlocked)
        {
            var size = originalSpec._size;

            var modifiedSpec = originalSpec._blockSpecs.ToArray();
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    if (x == 1 && y == 1) { continue; } // Keep the middle piece only

                    for (int z = 0; z < size.z; z++)
                    {
                        var index = x + size.x * (y + size.y * z);
                        modifiedSpec[index]._occupations = BlockOccupations.None;
                        modifiedSpec[index]._matterBelow = MatterBelow.Any;
                    }
                }
            }

            blockObjSpec._blocksSpec._blockSpecs = modifiedSpec;
        }
        else
        {
            blockObjSpec._blocksSpec = originalSpec;
        }
    }

    public void AfterPrefabLoad(PrefabGroupService prefabGroupService)
    {
        var botProject = projects.GetProject(ModUtils.PowerWheelBotUpId);
        var workerProject = projects.GetProject(ModUtils.PowerWheelWorkerUpId);
        var windmillProject = projects.GetProject(ModUtils.WindmillSizeUpId);

        foreach (var prefab in prefabGroupService.AllPrefabs)
        {
            var prefabSpec = prefab.GetComponent<PrefabSpec>();
            if (!prefabSpec) { continue; }

            ModifyPowerWheels(prefab, prefabSpec, botProject, workerProject);
            ModifyWindmills(prefab, prefabSpec, windmillProject);
        }
    }
}
