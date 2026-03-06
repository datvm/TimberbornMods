namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.ActivatorSystem.TimedComponentActivatorSpec")]
public record ParsedTimedComponentActivatorSpec(
    Boolean IsOptionallyActivable,
    Int32 CyclesUntilCountdownActivation,
    Single DaysUntilActivation,
    string ProgressBarActiveLabelLocKey,
    string ProgressBarNotActiveLabelLocKey,
    Boolean IsHazardousActivator
) : ParsedComponentSpec;