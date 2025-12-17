namespace ConfigurableToolGroups.UI.BuiltInRootProviders;

public abstract class BuiltInButtonCustomRootElement<T>(T button) : CustomBottomBarElement, IHotkeySupportedTool
    where T : IBottomBarElementsProvider
{
    public const int BuiltInReservedOrder = 1000;

    public override string Id { get; } = typeof(T).Name;

    public override IEnumerable<BottomBarElement> GetElements() => button.GetElements();

    public abstract IEnumerable<IToolHotkeyDefinition> GetHotkeys();
}
