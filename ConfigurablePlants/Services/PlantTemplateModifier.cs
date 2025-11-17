namespace ConfigurablePlants.Services;

public class PlantTemplateModifier(MSettings s) : ITemplateModifier, ILoadableSingleton
{
    bool modifyBos;
    BlockOccupations removingOccupation = BlockOccupations.None;

    public void Load()
    {
        if (s.RemoveCorner.Value) { removingOccupation |= BlockOccupations.Corners; }
        if (s.RemovePath.Value) { removingOccupation |= BlockOccupations.Path; }

        modifyBos = removingOccupation != BlockOccupations.None || s.WithoutGround.Value;
        removingOccupation = ~removingOccupation;
    }

    public EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original)
    {
        var isTree = template.Specs.FastAny(q => q is TreeComponentSpec);
        float modifier;

        template.TransformSpecs(spec =>
        {
            switch (spec)
            {
                case GrowableSpec g:
                    modifier = isTree ? s.TreeGrowthRate.Value : s.CropGrowthRate.Value;
                    
                    return modifier == 1f ? null : g with
                    {
                        GrowthTimeInDays = g.GrowthTimeInDays * modifier,
                    };
                case CuttableSpec c:
                    modifier = isTree ? s.TreeOutputMul.Value : s.CropOutputMul.Value;
                    
                    return modifier == 1f ? null : c with
                    {
                        YielderSpec = ModifyAmount(c.YielderSpec, modifier),
                    };
                case GatherableSpec g:
                    modifier = s.GatherableOutputMul.Value;
                    var growthMul = s.GatherableGrowthRate.Value;

                    return (modifier == 1f && growthMul == 1f) ? null : g with
                    {
                        YieldGrowthTimeInDays = g.YieldGrowthTimeInDays * growthMul,
                        YielderSpec = ModifyAmount(g.YielderSpec, modifier),
                    };
                case ReproducibleSpec r:
                    modifier = s.ReproducibleChanceMultiplier.Value;
                    
                    return modifier == 1f ? null : r with
                    {
                        ReproductionChance = r.ReproductionChance * modifier,
                    };
                case BlockObjectSpec bos when modifyBos:
                    return ModifyPlantBlockObjectSpec(bos);
            }

            return null;
        });

        return template;
    }

    BlockObjectSpec ModifyPlantBlockObjectSpec(BlockObjectSpec bos)
    {
        return bos with
        {
            BlocksSpec = bos.BlocksSpec with
            {
                BlockSpecs = [..bos.BlocksSpec.BlockSpecs.Select(bs =>
                {
                    return bs with
                    {
                        MatterBelow = bs.MatterBelow == MatterBelow.Ground ? MatterBelow.GroundOrStackable : bs.MatterBelow,
                        Occupations = bs.Occupations & removingOccupation,
                    };
                })],
            },
        };
    }

    static YielderSpec ModifyAmount(YielderSpec original, float mul)
    {
        return original with
        {
            Yield = original.Yield with
            {
                Amount = Mathf.FloorToInt(original.Yield.Amount * mul),
            }
        };
    }

    public bool ShouldModify(string blueprintName, string templateName, TemplateSpec originalTemplateSpec)
    {
        var bp = originalTemplateSpec.Blueprint;

        return bp.HasSpec<PlantableSpec>();
    }


}
