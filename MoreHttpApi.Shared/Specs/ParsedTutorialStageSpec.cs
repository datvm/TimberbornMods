namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.TutorialSystem.TutorialStageSpec")]
public record ParsedTutorialStageSpec(
    string Id,
    string Intro,
    string IntroLocKey
) : ParsedComponentSpec, IComponentSpecWithId;