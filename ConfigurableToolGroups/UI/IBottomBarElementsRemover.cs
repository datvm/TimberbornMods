namespace ConfigurableToolGroups.UI;

public interface IBottomBarElementsRemover
{
    ImmutableArray<Type> RemovingTypes { get; }
}
