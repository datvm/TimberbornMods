namespace TImprove4Ui.Services;

public class ObjectSelectionService(
    MSettings s,
    EventBus eb,
    Highlighter highlighter,
    EntityRegistry entities
) : ILoadableSingleton
{
    public void Load()
    {
        eb.Register(this);
    }

    [OnEvent]
    public void OnObjectSelected(SelectableObjectSelectedEvent ev)
    {
        if (!s.HighlightSimilar.Value) { return; }

        var prefab = ev.SelectableObject.GetComponentFast<PrefabSpec>();
        if (!prefab) { return; }

        var name = prefab.Name;

        var color = s.HighlightSimilarColor.Color;
        foreach (var e in entities.Entities)
        {
            var ePrefab = e.GetComponentFast<PrefabSpec>();
            if (!ePrefab || ePrefab == prefab || ePrefab.Name != name) { continue; }

            highlighter.HighlightSecondary(e, color);
        }
    }

    [OnEvent]
    public void OnObjectUnselected(SelectableObjectUnselectedEvent _)
    {
        highlighter.UnhighlightAllSecondary();
    }

}
