namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.NeedApplication.ProbabilityGroupSpec")]
public record ParsedProbabilityGroupSpec(
    string Id,
    Single Low,
    Single Medium,
    Single High
) : ParsedComponentSpec, IComponentSpecWithId;