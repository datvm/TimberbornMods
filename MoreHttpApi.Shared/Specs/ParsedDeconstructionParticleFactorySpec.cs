namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.DeconstructionSystem.DeconstructionParticleFactorySpec")]
public record ParsedDeconstructionParticleFactorySpec(
    Single MinParticleSpawnThreshold,
    Single MaxParticleSpawnThreshold,
    Int32 MinParticlesForThreshold,
    Int32 MaxParticlesForThreshold,
    string ParticlePrefabPath,
    Single MaxNeighboursCount
) : ParsedComponentSpec;