namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.TimbermeshAnimations.AnimatorState")]
public record ParsedAnimatorState(
    string StateName,
    string AnimationName,
    Single Speed,
    string SpeedModifier,
    Boolean Looped,
    ParsedAnimatorStateCondition[] Conditions
) : ParsedComponentSpec;