namespace ConfigurableFaction.UI;

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class ConfigurableFactionSettingsPanel : VisualElement
{

    public ConfigurableFactionSettingsPanel(
        DataAggregatorService dataAggregatorService
    )
    {
        dataAggregatorService.Initialize();


    }

}
