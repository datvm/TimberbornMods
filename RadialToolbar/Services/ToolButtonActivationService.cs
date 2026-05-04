namespace RadialToolbar.Services;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class ToolButtonActivationService(
    ToolGroupService toolGroupService,
    EventBus eb
) : ILoadableSingleton
{
    (ITool Tool, ToolGroupSpec Group)? pending;

    public void Load()
    {
        eb.Register(this);
    }

    public void ActivateButtonAndGroup(BottomBarButtonLookup<ToolButton> button)
    {
        var btn = button.Reference;
        var tool = btn.Tool;
        if (toolGroupService._assignedToolGroups.TryGetValue(tool, out var grp))
        {
            pending = (btn.Tool, grp);
            eb.Post(new ToolGroupEnteredEvent(grp));
        }

        button.Activate();
    }

    [OnEvent]
    public void OnToolExited(ToolExitedEvent e)
    {
        if (pending is null || e.Tool != pending.Value.Tool) { return; }

        eb.Post(new ToolGroupExitedEvent(pending.Value.Group));
        pending = null;
    }

}
