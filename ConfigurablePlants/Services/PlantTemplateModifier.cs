namespace ConfigurablePlants.Services;

public class PlantTemplateModifier(MSettings s) : ITemplateModifier, ILoadableSingleton
{
    bool modifyBos;
    BlockOccupations removingOccupation = BlockOccupations.None;
    float[][] plantModifiers = [];
    float[] productModifiers = [];

    int ITemplateModifier.Order => 100;

    public void Load()
    {
        plantModifiers = s.PlantGroupsValues;
        productModifiers = s.PlantGroupsValues[(int)MSettingPlantGroupType.Product];

        if (s.RemoveCorner.Value) { removingOccupation |= BlockOccupations.Corners; }
        if (s.RemovePath.Value) { removingOccupation |= BlockOccupations.Path; }

        modifyBos = removingOccupation != BlockOccupations.None || s.WithoutGround.Value;
        removingOccupation = ~removingOccupation;
    }

    public EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original)
    {
        var isTree = template.Specs.FastAny(q => q is TreeComponentSpec);
        var modifiers = isTree ? plantModifiers[(int)MSettingPlantGroupType.Tree] : plantModifiers[(int)MSettingPlantGroupType.Crop];

        float growthMod, outputMod, harvestMod;

        template.TransformSpecs(spec =>
        {
            switch (spec)
            {
                case PlantableSpec p:
                    growthMod = modifiers[(int)MSettingPlantGroupProperty.PlantingTime];
                    
                    return growthMod == 1f ? null : p with
                    {
                        PlantTimeInHours = p.PlantTimeInHours * growthMod,
                    };
                case GrowableSpec g:
                    growthMod = modifiers[(int)MSettingPlantGroupProperty.GrowthRate];
                    
                    return growthMod == 1f ? null : g with
                    {
                        GrowthTimeInDays = g.GrowthTimeInDays * growthMod,
                    };
                case CuttableSpec c:
                    harvestMod = modifiers[(int)MSettingPlantGroupProperty.HarvestTime];
                    outputMod = modifiers[(int)MSettingPlantGroupProperty.OutputMul];

                    return (harvestMod == 1f && outputMod == 1f) ? null : c with
                    {
                        Yielder = ModifyYielderSpec(c.Yielder, harvestMod, outputMod),
                    };
                case GatherableSpec g:
                    growthMod = productModifiers[(int)MSettingPlantGroupProperty.GrowthRate];
                    outputMod = productModifiers[(int)MSettingPlantGroupProperty.OutputMul];
                    harvestMod = productModifiers[(int)MSettingPlantGroupProperty.HarvestTime];

                    return (growthMod == 1f && outputMod == 1f && harvestMod == 1f) ? null : g with
                    {
                        YieldGrowthTimeInDays = g.YieldGrowthTimeInDays * growthMod,
                        Yielder = ModifyYielderSpec(g.Yielder, harvestMod, outputMod),
                    };
                case DemolishableSpec d:
                    harvestMod = modifiers[(int)MSettingPlantGroupProperty.DemolishTime];
                    
                    return harvestMod == 1f ? null : d with
                    {
                        DemolishTimeInHours = d.DemolishTimeInHours * harvestMod,
                    };
                case ReproducibleSpec r:
                    growthMod = s.ReproducibleChanceMultiplier.Value;
                    
                    return growthMod == 1f ? null : r with
                    {
                        ReproductionChance = r.ReproductionChance * growthMod,
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
            Blocks = [..bos.Blocks.Select(bs =>
                {
                    return bs with
                    {
                        MatterBelow = bs.MatterBelow == MatterBelow.Ground ? MatterBelow.GroundOrStackable : bs.MatterBelow,
                        Occupations = bs.Occupations & removingOccupation,
                    };
            })]
        };
    }

    static YielderSpec ModifyYielderSpec(YielderSpec original, float harvestTimeMul, float amountMul)
    {
        return original with
        {
            Yield = original.Yield with
            {
                Amount = Mathf.FloorToInt(original.Yield.Amount * amountMul),
            },
            RemovalTimeInHours = original.RemovalTimeInHours * harvestTimeMul,
        };
    }

    public bool ShouldModify(string blueprintName, string templateName, TemplateSpec originalTemplateSpec)
    {
        var bp = originalTemplateSpec.Blueprint;

        return bp.HasSpec<PlantableSpec>();
    }


}
