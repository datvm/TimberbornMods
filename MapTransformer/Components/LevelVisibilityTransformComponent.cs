namespace MapTransformer.Components;

[MultiBind(typeof(IMapResizeComponent), Contexts = BindAttributeContext.NonMenu)]
class LevelVisibilityTransformComponent(ILevelVisibilityService levelVisibility) : IMapResizeComponent
{
    public int Order => 100;

    public void Transform(in MapTransformContext ctx)
        => levelVisibility.ResetMaxVisibleLevel();
}
