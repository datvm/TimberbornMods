namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.UnityEngineSpecs.CollidersSpec")]
public record ParsedCollidersSpec(
    ParsedBoxColliderSpec[] BoxColliders,
    ParsedSphereColliderSpec[] SphereColliders,
    ParsedCapsuleColliderSpec[] CapsuleColliders
) : ParsedComponentSpec;