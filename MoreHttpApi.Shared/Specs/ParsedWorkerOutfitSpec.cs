namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.WorkerOutfitSystem.WorkerOutfitSpec")]
public record ParsedWorkerOutfitSpec(
    string Id,
    string FactionId,
    string WorkerType,
    string DiffuseTexture,
    string NormalTexture,
    string[] Attachments
) : ParsedComponentSpec;