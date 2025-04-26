global using System.Runtime.CompilerServices;
global using Timberborn.EnterableSystem;
global using Timberborn.PrefabGroupSystem;
global using Timberborn.SlotSystem;
global using Timberborn.WorkSystem;
global using Timberborn.MechanicalSystem;

namespace ConfigurableWorkplace.Patches;

public record OriginalWorkplaceValues(int MaxWorker, bool DisallowOthers)
{
    public int PowerOutput { get; set; }
}

[HarmonyPatch]
public static class WorkerPatches
{
    static readonly ConditionalWeakTable<GameObject, OriginalWorkplaceValues> ConfiguredPrefabs = [];

    [HarmonyPostfix, HarmonyPatch(typeof(PrefabGroupService), nameof(PrefabGroupService.Load))]
    public static void ModifyWorkplaces(PrefabGroupService __instance)
    {
        if (MSettings.MaxWorkerMul == 1 && !MSettings.BotEverywhere && ConfiguredPrefabs.Count() == 0) { return; }

        foreach (var prefab in __instance.AllPrefabs)
        {
            var workplace = prefab.GetComponent<WorkplaceSpec>();
            if (!workplace) { continue; }

            if (!ConfiguredPrefabs.TryGetValue(prefab, out var originalValues))
            {
                originalValues = new(workplace.MaxWorkers, workplace.DisallowOtherWorkerTypes);
                ConfiguredPrefabs.Add(prefab, originalValues);
            }

            if (originalValues.MaxWorker > 0)
            {
                var newMaxWorker = Math.Max(1, Mathf.FloorToInt(originalValues.MaxWorker * MSettings.MaxWorkerMul));
                if (newMaxWorker == workplace.MaxWorkers) { continue; }

                ModifyMaxWorker(workplace, newMaxWorker, originalValues);
                ModifyMaxWorkerPower(workplace, newMaxWorker, originalValues);
            }

            workplace._disallowOtherWorkerTypes = !MSettings.BotEverywhere && originalValues.DisallowOthers;
        }
    }

    static void ModifyMaxWorker(WorkplaceSpec wp, int maxWorker, OriginalWorkplaceValues originalValues)
    {
        wp._maxWorkers = maxWorker;

        var enterableSpec = wp.GetComponentFast<EnterableSpec>();
        if (!enterableSpec.LimitedCapacityFinished || enterableSpec.CapacityFinished == maxWorker) { return; }
        enterableSpec._capacityFinished = maxWorker;

        AddSlotsIfNeeded<PatrollingSlotInitializerSpec, PatrollingSlotSpec>(
            wp,
            originalValues.MaxWorker,
            comp => comp._patrollingSlots,
            maxWorker);

        AddSlotsIfNeeded<TransformSlotInitializerSpec, TransformSlotSpec>(
            wp,
            originalValues.MaxWorker,
            comp => comp._slots,
            maxWorker);
    }

    static void ModifyMaxWorkerPower(WorkplaceSpec wp, int maxWorker, OriginalWorkplaceValues originalValues)
    {
        var spec = wp.GetComponentFast<MechanicalNodeSpec>();
        if (!spec) { return; }

        if (originalValues.PowerOutput == 0)
        {
            originalValues.PowerOutput = spec._powerOutput;
        }

        var ratio = maxWorker / (float)originalValues.MaxWorker;
        var newPower = Mathf.RoundToInt(originalValues.PowerOutput * ratio);
        spec._powerOutput = newPower;
    }

    static void AddSlotsIfNeeded<TComp, TSpec>(WorkplaceSpec wp, int originalSlotCount, Func<TComp, List<TSpec>> listFunc, int neededSlots)
        where TComp : BaseComponent
        where TSpec : notnull, new()
    {
        var slotSpecs = wp.GetComponentFast<TComp>();
        if (!slotSpecs) { return; }

        var list = listFunc(slotSpecs);
        if (list.Count >= neededSlots) { return; }

        var addNeeded = neededSlots - list.Count;
        for (int i = 0; i < addNeeded; i++)
        {
            var newSlot = CopySerializable(list[i % originalSlotCount]);
            list.Add(newSlot);
        }
    }

    static readonly Dictionary<Type, ImmutableArray<FieldInfo>> fieldsCache = [];
    static T CopySerializable<T>(T src) where T : notnull, new()
    {
        var type = src.GetType(); // Don't use typeof<T>

        if (!fieldsCache.TryGetValue(type, out var fields))
        {
            fields = [.. type.GetFields(AccessTools.all)
                .Where(q =>!q.IsStatic && q.GetCustomAttribute<SerializeField>() is not null)];
        }

        var result = new T();

        foreach (var field in fields)
        {
            field.SetValue(result, field.GetValue(src));
        }

        return result;
    }

}
