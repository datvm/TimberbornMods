namespace ModdableToolGroupsDemo.UI;

public class OriginalPlantingButtonsRemover : IBottomBarElementsRemover
{
    public ImmutableArray<Type> RemovingTypes { get; } =
    [
        typeof(FieldsButtonCustomRootElement),
        typeof(ForestryButtonCustomRootElement),
    ];
}
