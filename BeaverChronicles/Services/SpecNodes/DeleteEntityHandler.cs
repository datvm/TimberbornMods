namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class DeleteEntityHandler(
    ILoc t,
    TerrainMap terrainMap,
    DestructionService destructionService
) : NodeHandlerBase<DeleteEntityData>
{
    public override string ForType => "DeleteEntity";

    protected override string? InternalHandleNode(DeleteEntityData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var entityIds = data.EntityIds
            .Select(controller.FormatText)
            .Where(id => id is not null)
            .Select(id => id!);

        string? beaverDeathMessage = null;
        HashSet<BlockObject> destroyingObjs = [];

        foreach (var entity in controller.GetEntities(entityIds))
        {
            if (entity.GetComponent<BlockObject>() is { } obj && obj)
            {
                destroyingObjs.Add(obj);
                node.LogVerbose(() => $"Object {obj.Name} {obj.GetEntityId()} is marked for destruction");
            }
            else if (entity.GetComponent<Mortal>() is { } mortal && mortal)
            {
                beaverDeathMessage ??= controller.FormatTextLoc(data.BeaverDeathMessageLoc)
                    ?? t.T("LV.BCEv.BeaverDeath");

                var name = KillCharacter(mortal, beaverDeathMessage);
                node.LogVerbose(() => $"Killed character {name} - {entity.GetEntityId()}");
            }
            else
            {
                Debug.LogWarning($"DeleteEntity node in event {controller.Spec.Id} referenced entity {entity.GetEntityId()} which is not a supported type for deletion." +
                    $"This should not happen. Supported types are currently buildings and characters. This entity will be skipped.");
            }
        }

        HashSet<Vector3Int> destroyingTerrains = data.TerrainBoundsValue is { } terrainBounds
            ? [.. GetTerrains(terrainBounds)]
            : [];

        if (destroyingObjs.Count > 0 || destroyingTerrains.Count > 0)
        {
            DestroyBuildingsAndTerrain(destroyingObjs, destroyingTerrains);

            if (destroyingTerrains.Count > 0)
            {
                node.LogVerbose(() => $"{destroyingTerrains.Count} terrain voxels destroyed");
            }
        }

        return node.NextNodeId;
    }

    void DestroyBuildingsAndTerrain(HashSet<BlockObject> destroyingObjs, HashSet<Vector3Int> destroyingTerrains)
    {
        var query = destructionService.QueryDestructingEntities(destroyingObjs, destroyingTerrains);
        destructionService.DestroyEntities(query);
    }

    string KillCharacter(Mortal mortal, string beaverDeathMessage)
    {
        var name = mortal.GetComponent<Character>().FirstName;
        mortal.DieInstantly(string.Format(beaverDeathMessage, name));

        return name;
    }

    IEnumerable<Vector3Int> GetTerrains(BoundsInt b)
    {
        var (x0, y0, z0) = b.min;
        var (x1, y1, z1) = b.max;

        for (int x = x0; x < x1; x++)
        {
            for (int y = y0; y < y1; y++)
            {
                for (int z = z0; z < z1; z++)
                {
                    if (terrainMap.IsTerrainVoxel(new(x, y, z)))
                    {
                        yield return new Vector3Int(x, y, z);
                    }
                }
            }
        }
    }
}