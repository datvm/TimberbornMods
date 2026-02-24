namespace DecorativePlants.Services;

[BindSingleton(Contexts = BindAttributeContext.MainMenu)]
public class PlantDataValidator(
    EventBus eb,
    DialogService diag,
    PanelStack panelStack
) : ILoadableSingleton
{
    static bool validated;

    public void Load() => eb.Register(this);

    [OnEvent]
    public void OnUIReady(PanelShownEvent _)
    {
        if (validated || panelStack.TopPanel.PanelController.GetType() != typeof(MainMenuPanel)) { return; }
        validated = true;

        Validate();
    }

    void Validate()
    {
        if (PersistentService.HasData()) { return; }
        diag.Alert("LV.DP.NoPlantData", true);
    }

}
