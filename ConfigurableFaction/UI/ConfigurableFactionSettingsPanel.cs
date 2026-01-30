namespace ConfigurableFaction.UI;

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class ConfigurableFactionSettingsPanel : VisualElement
{
    
    public ConfigurableFactionSettingsPanel(
        UserSettingsUIControllerScope controllerScope,
        IContainer container
    )
    {
        var controller = controllerScope.Controller;
        controller.Initialize();

        RegisterCallback<DetachFromPanelEvent>(_ => controllerScope.SaveAndUnload());

        var io = this.AddChild(container.GetInstance<SettingsIOPanel>).SetMarginBottom();

        var factionSelector = container.GetInstance<FactionSelector>();
        factionSelector.AddTo(this).SetMarginBottom();

        var factionSettings = this.AddChild(container.GetInstance<FactionsSettingsPanel>);
        factionSettings.Initialize();

        factionSelector.OnFactionSelected += controller.SetFaction;

        controller.SetFaction(factionSelector.SelectedFactionId);
    }

}
