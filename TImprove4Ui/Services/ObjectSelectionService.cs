namespace TImprove4Ui.Services;

public class ObjectSelectionService(
    MSettings s,
    EventBus eb,
    Highlighter highlighter,
    EntityRegistry entities,
    IBlockService blockService
) : ILoadableSingleton
{

    public void Load()
    {
        eb.Register(this);
    }

    [OnEvent]
    public void OnObjectSelected(SelectableObjectSelectedEvent ev)
    {
        var prefab = ev.SelectableObject.GetComponentFast<PrefabSpec>();
        if (!prefab) { return; }

        if (HighlightTubeways(prefab))
        {
            return;
        }

        HighlightSimilarObjects(prefab);
    }

    void HighlightSimilarObjects(PrefabSpec prefab)
    {
        if (!s.HighlightSimilar.Value) { return; }

        var name = prefab.Name;
        var color = s.HighlightSimilarColor.Color;
        foreach (var e in entities.Entities)
        {
            var ePrefab = e.GetComponentFast<PrefabSpec>();
            if (!ePrefab || ePrefab == prefab || ePrefab.Name != name) { continue; }

            highlighter.HighlightSecondary(e, color);
        }
    }

    bool IsTubeway(PrefabSpec obj) => obj.GetComponentFast<TubeSpec>() || obj.GetComponentFast<TubeStationSpec>(); 

    bool HighlightTubeways(PrefabSpec obj)
    {
        if (!s.HighlightTubeway.Value || !IsTubeway(obj)) { return false; }

        var startingObj = obj.GetComponentFast<BlockObject>();
        var color = s.HighlightSimilarColor.Color;

        Stack<Vector3Int> stack = new([startingObj.Coordinates]);
        HashSet<Vector3Int> visited = [];

        var neighbors = Deltas.Neighbors6Vector3Int;

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (visited.Contains(current)) { continue; }
            visited.Add(current);

            var blockObjs = blockService.GetObjectsAt(current);
            foreach (var block in blockObjs)
            {
                var prefab = block.GetComponentFast<PrefabSpec>();
                if (!prefab || !IsTubeway(prefab)) { continue; }

                if (prefab != obj)
                {
                    highlighter.HighlightSecondary(block, color);
                }

                foreach (var neighbor in neighbors)
                {
                    var neighborPos = current + neighbor;
                    if (!visited.Contains(neighborPos))
                    {
                        stack.Push(current + neighbor);
                    }
                }

                break; // There should be only one block in the same position
            }
        }

        return true;
    }

    [OnEvent]
    public void OnObjectUnselected(SelectableObjectUnselectedEvent _)
    {
        highlighter.UnhighlightAllSecondary();
    }

}
