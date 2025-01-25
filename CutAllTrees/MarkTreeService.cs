global using Timberborn.Forestry;
global using Timberborn.SingletonSystem;
using Timberborn.Persistence;
using Timberborn.TerrainSystem;
using UnityEngine;

namespace CutAllTrees;

public class MarkTreeService(
    ModSettings settings,
    TreeCuttingArea treeCuttingArea,
    ITerrainService terrain,
    ISingletonLoader singletonLoader
) : ILoadableSingleton, ISaveableSingleton
{
    const string SingletonId = $"{nameof(CutAllTrees)}.MarkTreeServiceActivated";

    static readonly SingletonKey singletonKey = new(SingletonId);
    static readonly PropertyKey<bool> activatedListKey = new("Activated");

    bool activated;

    public void Load()
    {
        if (!settings.Enabled) { return; }

        activated = singletonLoader.HasSingleton(singletonKey)
            && singletonLoader.GetSingleton(singletonKey).Get(activatedListKey);

        if (activated && !settings.AlwaysEnabled) { return; }

        MarkAllMapForCutting();
        activated = true;
        settings.AlwaysEnabled = false;
    }

    void MarkAllMapForCutting()
    {
        var size = terrain.Size;

        List<Vector3Int> coords = [];
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (terrain.TryGetCellHeight(new(x, y), out var h))
                {
                    coords.Add(new(x, y, h));
                }
            }
        }

        if (coords.Count > 0)
        {
            treeCuttingArea.AddCoordinates(coords);
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        singletonSaver.GetSingleton(singletonKey).Set(activatedListKey, activated);
    }
}
