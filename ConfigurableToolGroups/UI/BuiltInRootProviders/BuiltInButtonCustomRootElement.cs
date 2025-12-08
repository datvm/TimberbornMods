namespace ConfigurableToolGroups.UI.BuiltInRootProviders;

public abstract class BuiltInButtonCustomRootElement<T>(T button) : CustomBottomBarElement
    where T : IBottomBarElementsProvider
{
    public override string Id { get; } = typeof(T).Name;

    public override IEnumerable<BottomBarElement> GetElements() => button.GetElements();
}
