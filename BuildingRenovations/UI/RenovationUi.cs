namespace BuildingRenovations.UI;

static class RenovationUi
{
    public const string DecreasePriorityKey = "RenovationDecreasePriority";
    public const string IncreasePriorityKey = "RenovationIncreasePriority";

    public static PriorityToggleGroup CreatePriorityToggle(
        this PriorityToggleGroupFactory factory,
        VisualElement parent,
        BuilderPrioritySpriteLoader sprites,
        string labelLoc
    )
        => factory.Create(parent, labelLoc, sprites, DecreasePriorityKey, IncreasePriorityKey);
}
