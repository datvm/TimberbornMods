namespace ModdableToolGroupsDemo.UI;

public class OriginalDevButtonsRemover : IBottomBarElementsRemover
{
    public ImmutableArray<Type> RemovingTypes { get; } = [
        typeof(BeaverGeneratorButtonCustomRootElement),
        typeof(BotGeneratorButtonCustomRootElement),
        typeof(WaterHeightBrushButtonCustomRootElement),
    ];
}
