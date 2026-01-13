namespace TImprove4Ui.Services;

public class ObjectSelectionService(
    MSettings s,
    EventBus eb,
    Highlighter highlighter,
    EntityRegistry entities,
    IBlockService blockService,
    ISpecService specService
) : ILoadableSingleton
{
    BrushColorSpec brushColorSpec = null!;

    public void Load()
    {
        brushColorSpec = specService.GetSingleSpec<BrushColorSpec>();

        eb.Register(this);
    }

    [OnEvent]
    public void OnObjectSelected(SelectableObjectSelectedEvent ev)
    {
        if (!HighlightTubeways(ev.SelectableObject))
        {
            HighlightSimilarObjects(ev.SelectableObject);
        }
    }

    void HighlightSimilarObjects(BaseComponent comp)
    {
        if (!s.HighlightSimilar.Value) { return; }

        var template = comp.GetComponent<TemplateSpec>();
        var entity = comp.GetComponent<EntityComponent>();
        var name = template.TemplateName;
        var color = s.HighlightSimilarColor.Color;
        foreach (var e in entities.Entities)
        {
            var ePrefab = e.GetComponent<TemplateSpec>();
            if (ePrefab is null || ePrefab.TemplateName != name || e == entity) { continue; }

            highlighter.HighlightSecondary(e, color);
        }
    }

    bool IsSingleTube(BaseComponent obj)
    {
        if (!obj.HasComponent<TubeSpec>()) { return false; }

        var bo = obj.GetComponent<BlockObject>();
        return bo && bo._blockObjectSpec.Size == Vector3Int.one;
    }

    bool IsTubeway(BaseComponent obj) => obj.HasComponent<TubeSpec>() || obj.HasComponent<TubeStationSpec>();

    Color GetHighlightingTubewayColor(BaseComponent obj, Vector3Int coords)
    {
        if (!IsSingleTube(obj)) { return s.HighlightSimilarColor.Color; }

        var counter = 0;
        foreach (var n in Deltas.Neighbors6Vector3Int)
        {
            var curr = coords + n;

            var blockObjs = blockService.GetObjectsAt(curr);
            foreach (var block in blockObjs)
            {
                if (IsTubeway(block))
                {
                    counter++;
                    if (counter > 1) { break; }
                }
            }

            if (counter > 1) { break; }
        }
        
        return counter > 1 ? s.HighlightSimilarColor.Color : brushColorSpec.Negative;
    }

    bool HighlightTubeways(BaseComponent obj)
    {
        if (!s.HighlightTubeway.Value || !IsTubeway(obj)) { return false; }

        var startingObj = obj.GetComponent<BlockObject>();
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
                var template = block.GetComponent<TemplateSpec>();
                if (!block || !IsTubeway(block)) { continue; }

                if (block != startingObj)
                {
                    highlighter.HighlightSecondary(block, GetHighlightingTubewayColor(block, current));
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