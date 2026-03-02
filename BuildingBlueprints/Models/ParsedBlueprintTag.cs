namespace BuildingBlueprints.Models;

public readonly record struct ParsedBlueprintTag(string Name, ImmutableArray<ParsedBlueprintInfo> Blueprints);
public readonly record struct ParsedValidatedBlueprintTag(string Name, BlueprintWithValidation[] Blueprints);