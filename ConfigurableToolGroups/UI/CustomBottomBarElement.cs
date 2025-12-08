namespace ConfigurableToolGroups.UI;

public abstract class CustomBottomBarElement : IBottomBarElementsProvider
{
    public abstract string Id { get; }
    public CustomBottomBarElementSpec CustomBottomBarElementSpec { get; internal set; } = null!;

    public abstract IEnumerable<BottomBarElement> GetElements();
}
