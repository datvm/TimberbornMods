namespace BathBombs.UI;

public class BathBombFragment(
    VisualElementLoader visualElementLoader,
    ILoc t,
    InputService inputService,
    ITooltipRegistrar tooltipRegistrar
) : IEntityPanelFragment, IInputProcessor
{
#nullable disable
    VisualElement panel;
    Button btnDetonate;
#nullable enable

    BathBombComponent? comp;

    public void ClearFragment()
    {
        inputService.RemoveInputProcessor(this);
        panel.ToggleDisplayStyle(false);
        comp = null;
    }

    public VisualElement InitializeFragment()
    {
        panel = visualElementLoader.LoadVisualElement("Game/EntityPanel/DynamiteFragment");
        btnDetonate = panel.Q<Button>("Button");
        btnDetonate.RegisterCallback<ClickEvent>(DetonateSelectedDynamite);
        tooltipRegistrar.RegisterWithKeyBinding(btnDetonate, t.T(DynamiteFragment.DetonateLocKey), DynamiteFragment.UniqueBuildingActionKey);

        panel.ToggleDisplayStyle(false);
        return panel;
    }

    public bool ProcessInput()
    {
        if (!inputService.IsKeyDown(DynamiteFragment.UniqueBuildingActionKey)) { return false; }

        DetonateSelectedDynamite(null);
        return true;
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponent<BathBombComponent>();
        if (!comp)
        {
            ClearFragment();
            return;
        }

        panel.ToggleDisplayStyle(true);
        inputService.AddInputProcessor(this);
    }

    public void UpdateFragment()
    {
        if (!comp)
        {
            if (panel.IsDisplayed())
            {
                ClearFragment();
            }
        }
        else
        {
            btnDetonate.SetEnabled(!comp!.Triggered);
        }
    }

    void DetonateSelectedDynamite(ClickEvent? _)
    {
        if (!comp || comp!.Triggered) { return; }
        comp.Trigger();
    }
}
