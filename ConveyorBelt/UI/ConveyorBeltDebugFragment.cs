namespace ConveyorBelt.UI;

[BindSingleton] // Not Fragment, it's registered manually
public class ConveyorBeltDebugFragment(
    DebugFragmentFactory fragmentFac,
    DialogService diag
) : IEntityPanelFragment
{
    VisualElement root = null!;
    ConveyorBeltComponent? comp;

    public VisualElement InitializeFragment()
    {
        root = fragmentFac.Create([
            new(PushItem, "Push item to belt"),
            new(SetSpeed, "Set belt speed"),
        ]);
        return root;
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponent<ConveyorBeltComponent>();
        if (!comp)
        {
            ClearFragment();
            return;
        }

        root.SetDisplay(true);
    }

    public void ClearFragment()
    {
        comp = null;
        root.SetDisplay(false);
    }

    public void UpdateFragment() { }

    void PushItem()
    {
        if (!comp) { return; }

        if (comp!.CanAcceptItem("Log"))
        {
            comp.Push("Log");
        }
        else
        {
            Debug.LogWarning("Cannot push item to belt: item spacing or capacity limit not met.");
        }
    }

    async void SetSpeed()
    {
        if (!comp) { return; }

        var input = await diag.PromptAsync("Enter travelling time (hours):", comp!.Spec.TravelTimeHours.ToString());
        if (!float.TryParse(input, out var time)) { return; }

        comp.Spec.GetType().GetProperty(nameof(ConveyorBeltSpec.TravelTimeHours))!.SetValue(comp.Spec, time);
    }

}
