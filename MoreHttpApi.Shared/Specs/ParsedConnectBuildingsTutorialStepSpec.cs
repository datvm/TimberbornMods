namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.TutorialSteps.ConnectBuildingsTutorialStepSpec")]
public record ParsedConnectBuildingsTutorialStepSpec(
    string TemplateName,
    Int32 RequiredAmount,
    Boolean CountUnfinishedBuildings,
    string[] HighlightableBuildingIds
) : ParsedComponentSpec;