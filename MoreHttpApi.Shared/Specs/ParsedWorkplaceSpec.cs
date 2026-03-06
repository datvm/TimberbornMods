namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.WorkSystem.WorkplaceSpec")]
public record ParsedWorkplaceSpec(
    Int32 MaxWorkers,
    Int32 DefaultWorkers,
    string DefaultWorkerType,
    Boolean DisallowOtherWorkerTypes,
    ParsedWorkerTypeUnlockCost[] WorkerTypeUnlockCosts
) : ParsedComponentSpec;