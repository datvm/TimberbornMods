namespace Pin.UI;
public class PinUnderlay(
    RootVisualElementProvider rootVisualElementProvider,
    CameraService cameraService
) : ILoadableSingleton
{

    public VisualElement Root { get; private set; } = null!;
    readonly HashSet<PinPanel> panels = [];

    public void Load()
    {
        VisualElement e = rootVisualElementProvider.Create("Underlay", "Core/Underlay", 0);
        Root = e.Q<VisualElement>("Underlay");
    }

    public void Add(PinPanel panel)
    {
        Root.Add(panel);
        panels.Add(panel);
    }

    public void Remove(PinPanel panel)
    {
        panel.RemoveFromHierarchy();
        panels.Remove(panel);
    }

    public void UpdatePositions()
    {
        UpdatePositions(panels);
    }

    public void UpdatePositions(IEnumerable<PinPanel> panels)
    {
        var r = Root;
        var hw = r.layout.width / 2f;
        var hh = r.layout.height / 2f;

        foreach (var panel in panels)
        {
            var anchor = panel.Anchor;
            var isInFront = cameraService.IsInFront(anchor);
            panel.SetDisplay(isInFront);

            if (isInFront)
            {
                var pos = cameraService.WorldSpaceToPanelSpace(r, anchor);
                panel.style.translate = new Vector2(pos.x - hw, pos.y - hh);
            }
        }
    }

    public void ToggleVisibility() => Root.SetDisplay(!Root.IsDisplayed());

}
