namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.WorkerTypes.WorkerTypeSpec")]
public record ParsedWorkerTypeSpec(
    string Id,
    string[] BackwardCompatibleIds,
    string DisplayName,
    string WorkerOnlyText,
    Boolean IgnoresWorkingHours,
    string DisplayNameLocKey,
    string WorkerOnlyTextLocKey
) : ParsedComponentSpec, IComponentSpecWithId;