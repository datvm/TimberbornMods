namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.TutorialSystem.TutorialSpec")]
public record ParsedTutorialSpec(
    string Id,
    string DisplayName,
    string NameLocKey,
    string[] RequiredTutorialIds,
    string SkipIfTutorialFinished,
    Int32 SortOrder,
    string[] Stages
) : ParsedComponentSpec;