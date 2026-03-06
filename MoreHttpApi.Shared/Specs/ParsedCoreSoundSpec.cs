namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.CoreSound.CoreSoundSpec")]
public record ParsedCoreSoundSpec(
    Int32 MaxVerticalListenerPositionAboveGround,
    Single MinBuildingFadeDistance,
    Single MaxBuildingFadeDistance,
    string WindAmbientKey,
    Single MinAmbientFade,
    Single MaxAmbientFade,
    Single MinWindFade,
    Single MaxWindFade
) : ParsedComponentSpec;