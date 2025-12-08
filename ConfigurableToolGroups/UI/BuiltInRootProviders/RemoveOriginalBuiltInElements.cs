
namespace ConfigurableToolGroups.UI.BuiltInRootProviders;

public class RemoveOriginalBuiltInElements : IBottomBarElementsRemover
{
    public ImmutableArray<Type> RemovingTypes { get; } = [
        typeof(BeaverGeneratorButton),
        typeof(BotGeneratorButton),
        typeof(BuilderPrioritiesButton),
        typeof(CursorButton),
        typeof(DemolishingButton),
        typeof(FieldsButton),
        typeof(ForestryButton),
        typeof(TreeCuttingAreaButton),
        typeof(GameBlockObjectButtons),
        typeof(MapEditorBlockObjectButtons),
        typeof(MapEditorToolButtons),
        typeof(ShowOptionsButton),
        typeof(WaterHeightBrushButton),
    ];
}
