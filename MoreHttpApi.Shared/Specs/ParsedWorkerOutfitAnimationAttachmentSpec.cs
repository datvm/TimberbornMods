namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.WorkerOutfitSystem.WorkerOutfitAnimationAttachmentSpec")]
public record ParsedWorkerOutfitAnimationAttachmentSpec(
    string WorkerOutfit,
    string[] AnimationNames,
    string[] ShowWhenActive,
    string[] HideWhenActive
) : ParsedComponentSpec;