namespace ConfigurableToolGroups.UI.BuiltInRootProviders;

public abstract class GroupedBuiltInButtonCustomRootElement<T>(T button, ToolButtonService toolButtonService)
    : BuiltInButtonCustomRootElement<T>(button)
    where T : IBottomBarElementsProvider
{
    public const string PlantingGroup = "Plantings";

    protected abstract int ReservedOrder { get; }
    protected abstract string ToolGroupId { get; }
    protected abstract ToolHotkeyDefinitionBase? GetHotkeyDefinition(ToolButton btn);

    public override IEnumerable<IToolHotkeyDefinition> GetHotkeys()
    {
        // The group
        var grpBtn = toolButtonService._toolGroupButtons.First(q => q._toolGroup.Id == ToolGroupId);
        var toolGrp = grpBtn._toolGroup;

        var hkId = $"ToolGroup.{toolGrp.Id}";
        var grpHk = new ButtonToolHotkeyDefinition(hkId, toolGrp.DisplayNameLocKey, grpBtn)
        {
            Order = BuiltInReservedOrder + ReservedOrder,
        };

        if (this is FieldsButtonCustomRootElement || this is ForestryButtonCustomRootElement)
        {
            grpHk.GroupId = PlantingGroup;
        }

        yield return grpHk;

        // The tools in the group
        var counter = 0;
        foreach (var btn in grpBtn.ToolButtons)
        {
            var def = GetHotkeyDefinition(btn);
            if (def is not null)
            {
                if (def.Order is null)
                {
                    counter += 10;
                    def.Order = BuiltInReservedOrder + ReservedOrder + counter;
                }
                
                yield return def;
            }
        }
    }

}
