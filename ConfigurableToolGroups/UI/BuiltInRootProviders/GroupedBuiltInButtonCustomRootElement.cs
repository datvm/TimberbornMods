namespace ConfigurableToolGroups.UI.BuiltInRootProviders;

public class GroupedBuiltInButtonCustomRootElementDI(
    ToolButtonService ToolButtonService,
    BottomBarButtonLookupService LookupService,
    ILoc T
)
{
    public readonly ToolButtonService ToolButtonService = ToolButtonService;
    public readonly BottomBarButtonLookupService LookupService = LookupService;
    public readonly ILoc T = T;
}

public abstract class GroupedBuiltInButtonCustomRootElement<T>(
    T button,
    GroupedBuiltInButtonCustomRootElementDI di
) : BuiltInButtonCustomRootElement<T>(button)
    where T : IBottomBarElementsProvider
{
    protected readonly ToolButtonService toolButtonService = di.ToolButtonService;
    protected readonly BottomBarButtonLookupService lookupService = di.LookupService;
    protected readonly ILoc t = di.T;

    public const string PlantingGroup = "Plantings";

    protected abstract int ReservedOrder { get; }
    protected abstract string ToolGroupId { get; }
    protected abstract ToolHotkeyDefinitionBase? GetHotkeyDefinition(ToolButton btn);

    protected virtual void RegisterToolGroupButton(VisualElement el) => RegisterToolGroup(el);
    protected abstract void RegisterToolButton(VisualElement el, ToolButton btn);

    public override IEnumerable<BottomBarElement> GetElements()
    {
        foreach (var el in base.GetElements())
        {
            RegisterToolGroupButton(el.MainElement);

            if (el.SubElement is not null)
            {
                foreach (var childEl in el.SubElement.Children())
                {
                    if (lookupService.TryGetToolButton(childEl, out var btn))
                    {
                        RegisterToolButton(childEl, btn);
                    }
                }
            }

            yield return el;
        }
    }

    protected void RegisterToolGroup(VisualElement btn)
    {
        if (!lookupService.TryGetToolGroupButton(btn, out var tgBtn)) { return; }

        var toolGroup = tgBtn._toolGroup;
        btn.AddToClassList("tool-group--" + toolGroup.DisplayNameLocKey);
        lookupService.Register(tgBtn);
    }

    protected void RegisterTool(ToolButton toolBtn, string id, string titleLoc)
    {
        lookupService.Register(toolBtn, "ToolButton-" + id, titleLoc);
        toolBtn.Root.AddToClassList("tool--" + id);
    }

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
