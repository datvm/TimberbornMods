namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.Workshops.RecipeSpec")]
public record ParsedRecipeSpec(
    string Id,
    string[] BackwardCompatibleIds,
    string DisplayLocKey,
    Single CycleDurationInHours,
    ParsedGoodAmountSpec[] Ingredients,
    ParsedGoodAmountSpec[] Products,
    Int32 ProducedSciencePoints,
    string Fuel,
    Int32 CyclesFuelLasts,
    Int32 FuelCapacity,
    Int32 CyclesCapacity,
    string Icon
) : ParsedComponentSpec;