namespace Timberborn.BlockSystem;

public static class ScenarioEditorExtensions
{

    public static Placement GetPlacement<T>(this T spec) where T : BaseComponent
        => spec.GetComponentFast<BlockObject>().Placement;

}
