namespace NoBuildRestriction.Services;

public class RestrictionTemplateModifier : ITemplateModifier
{
    static readonly FrozenSet<string> Excluded1x1Buildings = ["TerrainBlock.Folktails", "TerrainBlock.IronTeeth", "Dynamite.Folktails", "Dynamite.IronTeeth"];
    static readonly FrozenSet<string> HangingStructureExclusions = ["MechanicalFluidPump.Folktails", "DeepMechanicalFluidPump.IronTeeth"];


    public EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original)
    {
        var hasContinuousTerrainConstraintSpec = false;
        var isBuilding = false;
        var bosIndex = -1;
        BlockObjectSpec? bos = null;
        var hasFloodBlocker = false;
        PlaceableBlockObjectSpec? pbos = null;
        int pbosIndex = -1;

        for (int i = 0; i < template.Specs.Count; i++)
        {
            var s = template.Specs[i];

            switch (s)
            {
                case BuildingSpec:
                    isBuilding = true;
                    break;
                case ContinuousTerrainConstraintSpec:
                    hasContinuousTerrainConstraintSpec = true;
                    break;
                case BlockObjectSpec spec:
                    bos = spec;
                    bosIndex = i;
                    break;
                case FloodableObjectBlockerSpec:
                    hasFloodBlocker = true;
                    break;
                case PlaceableBlockObjectSpec spec:
                    pbos = spec;
                    pbosIndex = i;
                    break;
                case UnderstructureConstraintSpec:
                    return null; // Do not modify templates with UnderstructureConstraintSpec
            }
        }

        if (bos is null || pbos is null) { return null; } // Should not happen

        BlockSpec[] bss = [.. bos.Blocks];

        // Bottom of map
        if (MSettings.NoBottomOfMap && hasContinuousTerrainConstraintSpec)
        {
            RemoveContinuousTerrainConstraintOccupation(bss);
        }

        if (!isBuilding) { goto RETURN; }

        // Allow flooded buildings to work
        if (MSettings.AllowFlooded && !hasFloodBlocker)
        {
            template.Specs.Add(new FloodableObjectBlockerSpec());
        }

        // Other restrictions
        RemoveBuildingRestrictions(bss, bos, originalTemplateSpec.TemplateName);

        // Super foundations
        if (MSettings.SuperStructure)
        {
            AddSuperFoundation(bss, bos, originalTemplateSpec.TemplateName, pbos,
                replacement => template.Specs[pbosIndex] = pbos = replacement);
        }

    RETURN:
        template.Specs[bosIndex] = bos with
        {
            Blocks =  [.. bss],
        };

        return template;
    }

    static void RemoveContinuousTerrainConstraintOccupation(BlockSpec[] bss)
    {
        for (int i = 0; i < bss.Length; i++)
        {
            bss[i] = bss[i] with { OccupyAllBelow = false };
        }
    }

    static void RemoveBuildingRestrictions(BlockSpec[] bss, BlockObjectSpec bos, string templateName)
    {
        var size = bos.Size;
        var (sx, sy, _) = size;

        RemovePlacementRestrictions(bss, bos);

        if (MSettings.PlatformOver1x1 && sx == 1 && sy == 1 && !Excluded1x1Buildings.Contains(templateName))
        {
            Remove1x1Corners(bss);
        }

        if (MSettings.AlwaysSolid)
        {
            AddSolidTop(bss, size);
        }
    }

    static void RemovePlacementRestrictions(BlockSpec[] bss, BlockObjectSpec bos)
    {
        var shouldRemoveGround = MSettings.RemoveGroundOnly && bss.FastAll(q => !q.Underground);

        for (int i = 0; i < bss.Length; i++)
        {
            switch (bss[i].MatterBelow)
            {
                case MatterBelow.Ground when shouldRemoveGround:
                    bss[i] = bss[i] with { MatterBelow = MatterBelow.GroundOrStackable };
                    break;
                case MatterBelow.Stackable when MSettings.RemoveRoofOnly:
                    bss[i] = bss[i] with { MatterBelow = MatterBelow.GroundOrStackable };
                    break;
            }
        }
    }

    static void Remove1x1Corners(BlockSpec[] bss)
    {
        const BlockOccupations RemovingCorner = ~BlockOccupations.Corners;

        for (int z = bss.Length - 1; z >= 0; z--)
        {
            var curr = bss[z];
            if (z == 0 && curr.Occupations == BlockOccupations.Corners)
            {
                // Don't remove the bottom corner-only block
                // Or else it's floating and cause building site to crash
                continue;
            }

            if (curr.Occupations.HasFlag(BlockOccupations.Corners))
            {
                bss[z] = curr with
                {
                    Occupations = curr.Occupations & RemovingCorner
                };
            }
        }
    }

    static void AddSolidTop(BlockSpec[] bss, in Vector3Int size)
    {
        // Must have something different more than bottom parts
        const BlockOccupations RequiredDifferenceFrom = BlockOccupations.Bottom | BlockOccupations.Path | BlockOccupations.Floor;

        var (sx, sy, sz) = size;

        var index = 0;
        for (int x = 0; x < sx; x++)
        {
            for (int y = 0; y < sy; y++)
            {
                for (int z = 0; z < sz; z++)
                {
                    var curr = bss[index];

                    if (curr.Stackable != BlockStackable.BlockObject && (curr.Occupations | RequiredDifferenceFrom) != RequiredDifferenceFrom)
                    {
                        bss[index] = curr with
                        {
                            Stackable = BlockStackable.BlockObject,
                        };
                    }

                    index++;
                }
            }
        }
    }

    static void AddSuperFoundation(BlockSpec[] bss, BlockObjectSpec bos, string templateName, PlaceableBlockObjectSpec pbos, Action<PlaceableBlockObjectSpec> replacePbos)
    {
        var (sx, sy, sz) = bos.Size;
        var mainX = sx / 2;
        var mainY = sy / 2;

        if (bos.Entrance.HasEntrance)
        {
            var entrance = bos.Entrance.Coordinates;
            mainX = entrance.x;
            mainY = entrance.y + 1;
        }

        var attachToTerrain = pbos.CanBeAttachedToTerrainSide ||
            (MSettings.MagicStructure && MSettings.HangingStructure
            && (sx == 1 || sy == 1) // Only for 1xn or nx1 structures
            && !HangingStructureExclusions.Contains(templateName));

        var pivot = pbos.CustomPivot.HasCustomPivot
            ? pbos.CustomPivot.Coordinates
            : new Vector3(mainX + .5f, mainY + .5f, 0);

        replacePbos(pbos with
        {
            CustomPivot = pbos.CustomPivot with
            {
                HasCustomPivot = true,
                Coordinates = pivot,
            },

            CanBeAttachedToTerrainSide = attachToTerrain,
        });

        if (MSettings.MagicStructure)
        {
            mainX = mainY = -1;
        }

        var index = 0;
        for (int z = 0; z < sz; z++) // For some sadistic reason, blocks are stored in ZYX order
        {
            for (int y = 0; y < sy; y++)
            {
                for (int x = 0; x < sx; x++)
                {
                    if (x == mainX && y == mainY)
                    {
                        index++;
                        continue;
                    }

                    var curr = bss[index];
                    if (curr.MatterBelow != MatterBelow.Any)
                    {
                        bss[index] = curr with
                        {
                            MatterBelow = MatterBelow.Any,
                        };
                    }
                    index++;
                }
            }
        }
    }

    public bool ShouldModify(string blueprintName, string templateName, TemplateSpec originalTemplateSpec)
        => MSettings.ModifyObjects;
}
