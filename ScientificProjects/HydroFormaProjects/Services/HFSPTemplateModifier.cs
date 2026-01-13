
namespace HydroFormaProjects.Services;

public class HFSPTemplateModifier(ScientificProjectUnlockRegistry unlocks) : ITemplateModifier
{
    static readonly Directions3D AllDirections = (Directions3D)(1 + 2 + 4 + 8 + 0x10 + 0x20);

    EditableBlueprint ModifyDam(EditableBlueprint template)
    {
        template.Specs.RemoveAll(spec => spec is FinishableWaterObstacleSpec);
        template.Specs.Add(new DamGateComponentSpec());

        return template;
    }

    EditableBlueprint ModifyLevee(EditableBlueprint template)
    {
        template.Specs.Add(new MechanicalNodeSpec()
        {
            IsShaft = true
        });

        template.Specs.Add(new TransputProviderSpec()
        {
            Transputs = [
                new()
                {
                    Coordinates = Vector3Int.zero,
                    Directions = AllDirections,
                }
            ],
        });

        template.Specs.Add(new ShaftSoundEmitterSpec());

        return template;
    }

    EditableBlueprint ModifyDirtExcavator(EditableBlueprint template)
    {
        var workplaceIndex = template.Specs.FindIndex(q => q is WorkplaceSpec);
        var workplace = (WorkplaceSpec)template.Specs[workplaceIndex];
        var modifier = 1f / workplace.MaxWorkers;

        template.Specs[workplaceIndex] = workplace with
        {
            MaxWorkers = 1,
            DefaultWorkers = 1,
        };

        template.Specs.Add(new RecipeTimeMultiplierSpec()
        {
            Id = "HFDirtExcavatorUpgrade",
            Multiplier = modifier,
        });

        return template;
    }

    EditableBlueprint ModifyBarriers(EditableBlueprint template)
    {
        const BlockOccupations RemovePath = ~BlockOccupations.Path;

        template.TransformSpec<BlockObjectSpec>(blockSpec => blockSpec with
        {
            Blocks = [..blockSpec.Blocks.Select(value => value with
                {
                    Occupations = value.Occupations & RemovePath,
                })]
        });

        return template;
    }

    EditableBlueprint ModifyImpermeableFloor(EditableBlueprint template)
    {
        template.TransformSpecs(static spec => spec switch
            {
                BlockObjectNavMeshSettingsSpec nav => nav with
                {
                    EdgeGroups =
                    [
                        new()
                        {
                            Cost = .25f,
                            IsPath = true,
                            AddedEdges = [
                                new() { End = new(0,-1,0) },
                                new() { End = new(0,1,0) },
                                new() { End = new(1,0,0) },
                                new() { End = new(-1,0,0) },
                            ],
                            Group = new(),
                        }
                    ],
                },
                PlaceableBlockObjectSpec pbos => pbos with
                {
                    CanBeAttachedToTerrainSide = true,
                },
                BlockObjectSpec bos => bos with
                {
                    Blocks = [..bos.Blocks.Select(value => value with
                    {
                        MatterBelow = MatterBelow.Any,
                    })]
                },
                _ => null,
            });

        return template;
    }

    public bool ShouldModify(string blueprintName, string templateName, TemplateSpec originalTemplateSpec) => templateName switch
    {
        "Dam.Folktails" or "Dam.IronTeeth" => true,
        "Levee.Folktails" or "Levee.IronTeeth" => unlocks.Contains(HydroFormaModUtils.LeveeUpgrade),
        "DirtExcavator.Folktails" or "DirtExcavator.IronTeeth" => unlocks.Contains(HydroFormaModUtils.DirtExcavatorUpgrade),
        "ContaminationBarrier.Folktails" or "IrrigationBarrier.IronTeeth" => unlocks.Contains(HydroFormaModUtils.BarrierUpgrade),
        "ImpermeableFloor.Folktails" or "ImpermeableFloor.IronTeeth" => unlocks.Contains(HydroFormaModUtils.ImpermeableFloorUpgrade),
        _ => false,
    };

    public EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original)
        => originalTemplateSpec.TemplateName switch
        {
            "Dam.Folktails" or "Dam.IronTeeth" => ModifyDam(template),
            "Levee.Folktails" or "Levee.IronTeeth" => ModifyLevee(template),
            "DirtExcavator.Folktails" or "DirtExcavator.IronTeeth" => ModifyDirtExcavator(template),
            "ContaminationBarrier.Folktails" or "IrrigationBarrier.IronTeeth" => ModifyBarriers(template),
            "ImpermeableFloor.Folktails" or "ImpermeableFloor.IronTeeth" => ModifyImpermeableFloor(template),
            _ => null,
        };
}
