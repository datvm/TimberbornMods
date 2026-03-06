namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.BotUpkeep.BotManufactoryAnimationControllerSpec")]
public record ParsedBotManufactoryAnimationControllerSpec(
    Single AssemblyDuration,
    Single RingRotationSpeed,
    Single DrillRotationSpeed,
    string RingName,
    string DrillName,
    string[] AttachmentIds
) : ParsedComponentSpec;