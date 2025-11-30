global using Timberborn.EntitySystem;
global using Timberborn.Growing;

namespace TImprove4Modders.DevModules;

public class PlantModule(EntityRegistry entities) : IDevModule
{

    public DevModuleDefinition GetDefinition()
    {
        return new DevModuleDefinition.Builder()
            .AddMethod(DevMethod.Create("Plants: Set growth to 0%", GrowthTo0))
            .AddMethod(DevMethod.Create("Plants: Set growth to 25%", GrowthTo25))
            .AddMethod(DevMethod.Create("Plants: Set growth to 50%", GrowthTo50))
            .AddMethod(DevMethod.Create("Plants: Set growth to 75%", GrowthTo75))
            .AddMethod(DevMethod.Create("Plants: Set growth to 100%", GrowthTo100))
            .AddMethod(DevMethod.Create("Plants: Add 10% growth", AddGrowth10))
            .AddMethod(DevMethod.Create("Plants Gatherables: Add 10% progress", () => AddGatherableProgress(.1f)))
            .AddMethod(DevMethod.Create("Plants Gatherables: Add 100% progress", () => AddGatherableProgress(1f)))
            .Build();
    }

    void GrowthTo0() => GrowthTo(0);
    void GrowthTo25() => GrowthTo(.25f);
    void GrowthTo50() => GrowthTo(.50f);
    void GrowthTo75() => GrowthTo(.75f);
    void GrowthTo100() => GrowthTo(1f);

    void AddGrowth10() => GrowthTo(.1f, true);

    void GrowthTo(float progress, bool add = false)
    {
        var count = 0;
        var growables = 0;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        foreach (var item in entities.Entities)
        {
            count++;

            var growable = item.GetComponent<Growable>();
            if (!growable) { continue; }
            growables++;

            if (!add)
            {
                growable._timeTrigger.Reset();
                growable._timeTrigger.Resume();
            }

            if (progress > 0)
            {
                growable._timeTrigger.FastForwardProgress(progress);
            }
        }

        sw.Stop();
        Debug.Log($"Processed {growables} growable entities out of {count} entities in {sw.ElapsedMilliseconds}ms.");
    }

    void AddGatherableProgress(float progress)
    {
        var count = 0;
        var gatherables = 0;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        foreach (var item in entities.Entities)
        {
            count++;
            var yieldComponent = item.GetComponent<GatherableYieldGrower>();
            if (!yieldComponent) { continue; }
            gatherables++;


            if (yieldComponent._growable.IsGrown)
            {
                yieldComponent._timeTrigger.FastForwardProgress(progress);
            }
        }

        sw.Stop();
        Debug.Log($"Processed {gatherables} plant gatherables entities out of {count} entities in {sw.ElapsedMilliseconds}ms.");
    }

}
