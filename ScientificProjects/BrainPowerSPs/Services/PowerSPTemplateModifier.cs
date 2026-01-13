namespace BrainPowerSPs.Services;

public class PowerSPTemplateModifier(
    ScientificProjectUnlockRegistry unlocks,
    ScientificProjectRegistry registry
) : ITemplateModifier, ILoadableSingleton
{
    bool hasBotUpgrade;
    bool hasWorkerUpgrade;
    bool hasWindmillUpgrade;

    static readonly FrozenSet<string> WindTurbineTemplates = ["WindTurbine.Folktails", "LargeWindTurbine.Folktails"];

    public void Load()
    {
        hasBotUpgrade = unlocks.Contains(PowerProjectsUtils.PowerWheelBotUpId);
        hasWorkerUpgrade = unlocks.Contains(PowerProjectsUtils.PowerWheelWorkerUpId);
        hasWindmillUpgrade = unlocks.Contains(PowerProjectsUtils.WindmillSizeUpId);
    }

    public bool ShouldModify(string blueprintName, string templateName, TemplateSpec originalTemplateSpec)
        => (templateName == "PowerWheel.Folktails" && hasBotUpgrade)
        || (templateName == "LargePowerWheel.IronTeeth" && (hasWorkerUpgrade || hasBotUpgrade))
        || (hasWindmillUpgrade && WindTurbineTemplates.Contains(templateName));

    public EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original)
        => WindTurbineTemplates.Contains(originalTemplateSpec.TemplateName) ? ModifyWindmills(template) : ModifyPowerWheel(template);

    EditableBlueprint? ModifyWindmills(EditableBlueprint template)
    {
        template.TransformSpec<BlockObjectSpec>(blockObj => blockObj with
        {
            Blocks = [.. TransformWindmill(blockObj)],
        });

        return template;

        static IEnumerable<BlockSpec> TransformWindmill(BlockObjectSpec blockObj)
        {
            var spec = blockObj.Blocks;
            var (sx, sy, sz) = blockObj.Size;

            var index = -1;
            for (int z = 0; z < sz; z++)
            {
                for (int y = 0; y < sy; y++)
                {
                    for (int x = 0; x < sx; x++)
                    {
                        ++index;
                        
                        var curr = spec[index];

                        if (!(x == 1 && y == 1))
                        {
                            curr = curr with
                            {
                                MatterBelow = MatterBelow.Any,
                                Occupations = BlockOccupations.None,
                                Stackable = BlockStackable.None,
                            };
                        }

                        yield return curr;
                    }
                }
            }
        }
    }

    EditableBlueprint? ModifyPowerWheel(EditableBlueprint template)
    {
        template.TransformSpec<WorkplaceSpec>(workplace =>
        {
            var maxWorkers = workplace.MaxWorkers;
            var defaultWorkers = workplace.DefaultWorkers;

            if (hasWorkerUpgrade)
            {
                var spec = registry.GetProject(PowerProjectsUtils.PowerWheelWorkerUpId);
                var reducing = (int)spec.Parameters[0];
                if (maxWorkers > reducing)
                {
                    maxWorkers -= reducing;
                    defaultWorkers = Math.Min(defaultWorkers, maxWorkers);
                }
            }

            return workplace with
            {
                DisallowOtherWorkerTypes = !hasBotUpgrade && workplace.DisallowOtherWorkerTypes,
                MaxWorkers = maxWorkers,
                DefaultWorkers = defaultWorkers,
            };
        });

        return template;
    }


}
