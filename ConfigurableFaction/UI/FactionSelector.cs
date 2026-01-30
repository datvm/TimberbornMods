namespace ConfigurableFaction.UI;

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class FactionSelector(
    DataAggregatorService aggregator,
    VisualElementInitializer veInit,
    DropdownItemsSetter setter
)
{

    public string SelectedFactionId { get; private set; } = "";
    public event Action<string> OnFactionSelected = null!;

    public Dropdown AddTo(VisualElement parent)
    {
        var cbo = parent.AddDropdown();

        cbo.Initialize(veInit);

        var list = aggregator.Factions.Items
            .Select(f => f.Spec.DisplayName.Value)
            .ToArray();

        cbo.SetItems(setter, list, list[0]);
        SelectedFactionId = aggregator.Factions.Items[0].Id;

        cbo.AddChangeHandler((_, i) =>
        {
            var id = aggregator.Factions.Items[i].Id;
            SelectedFactionId = id;
            OnFactionSelected(id);            
        });

        return cbo;
    }

}
