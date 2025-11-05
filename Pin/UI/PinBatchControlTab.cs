
namespace Pin.UI;

public class PinBatchControlTab(
    VisualElementLoader visualElementLoader,
    BatchControlDistrict batchControlDistrict,
    EventBus eventBus,
    IContainer container,
    PinService pinService
) : BatchControlTab(visualElementLoader, batchControlDistrict, eventBus)
{
    public override string TabNameLocKey { get; } = "LV.Pin.Pins";
    public override string TabImage { get; } = "Pins";
    public override string BindingKey { get; } = "PinsTab";

    public override VisualElement GetHeader()
    {
        var grp = new VisualElement();

        foreach (var pin in pinService.Pins)
        {
            var row = grp.AddChild(container.GetInstance<PinBatchControlRow>);
            row.SetPin(pin);
        }

        return grp;
    }

    public override IEnumerable<BatchControlRowGroup> GetRowGroups(IEnumerable<EntityComponent> entities) => [];

}
