namespace BlueprintReader.Specs;

public record YielderSpec(
    YielderYieldSpec Yield,
    string ResourceGroup
);

public record YielderYieldSpec(string Id, int Amount);
