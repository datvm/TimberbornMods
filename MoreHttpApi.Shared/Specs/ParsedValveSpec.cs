namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.WaterBuildings.ValveSpec")]
public record ParsedValveSpec(
    Single MaxOutflowLimit,
    Single OutflowLimitStep,
    Boolean DefaultOutflowLimitEnabled,
    Single DefaultOutflowLimit,
    Boolean DefaultAutomationOutflowLimitEnabled,
    Single DefaultAutomationOutflowLimit,
    Single RateOfChangeHighPrimary,
    Single RateOfChangeHighSecondary,
    Single RateOfChangeLowPrimary,
    Single RateOfChangeLowSecondary,
    Int32 RateOfChangePrimaryTicks,
    Int32 RateOfChangePrimaryToSecondaryTicks,
    Single ReactionSpeedExponent,
    Single ReactionSpeedStep
) : ParsedComponentSpec;