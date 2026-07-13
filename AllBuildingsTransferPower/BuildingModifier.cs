using ModdableTimberborn.DependencyInjection;

namespace AllBuildingsTransferPower;

[MultiBind(typeof(ITemplateModifier))]
public class BuildingModifier : ITemplateModifier
{
    public EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original)
    {
        var bo = template.GetSpec<BlockObjectSpec>();
        if (bo is null) { return null; }

        var blocks = GetBuildingBlocks(bo).ToDictionary(b => b.Coords, b => b.IsBorder);

        var originalIndex = template.Specs.FindIndex(spec => spec is TransputProviderSpec);
        var ignoreRotation = false;
        HashSet<(Vector3Int Coordinates, Direction3D Direction)> reverseRotations = [];
        if (originalIndex >= 0)
        {
            var tps = (TransputProviderSpec)template.Specs[originalIndex];
            ignoreRotation = tps.IgnoreRotation;

            foreach (var transput in tps.Transputs)
            {
                if (!transput.ReverseRotation) { continue; }

                foreach (var direction in GetDirections(transput.Directions))
                {
                    reverseRotations.Add((transput.Coordinates, direction));
                }
            }

            template.Specs.RemoveAt(originalIndex);
        }

        if (!template.Specs.Any(s => s is MechanicalConnectorTargetSpec))
        {
            template.Specs.Add(new MechanicalConnectorTargetSpec());
        }

        List<TransputSpec> transputs = [];
        foreach (var (coords, isBorder) in blocks)
        {
            if (!isBorder) { continue; }

            var directions = FindDirections(coords, blocks);
            var normalDirections = Directions3D.None;
            var reversedDirections = Directions3D.None;

            foreach (var direction in GetDirections(directions))
            {
                if (reverseRotations.Contains((coords, direction)))
                {
                    reversedDirections |= direction.ToDirections3D();
                }
                else
                {
                    normalDirections |= direction.ToDirections3D();
                }
            }

            AddTransput(transputs, coords, normalDirections, reverseRotation: false);
            AddTransput(transputs, coords, reversedDirections, reverseRotation: true);
        }

        template.Specs.Add(new TransputProviderSpec()
        {
            Transputs = [.. transputs],
            IgnoreRotation = ignoreRotation,
        });

        return template;
    }

    public bool ShouldModify(string blueprintName, string templateName, TemplateSpec originalTemplateSpec)
    {
        if (!originalTemplateSpec.HasSpec<PlaceableBlockObjectSpec>()
            || originalTemplateSpec.HasSpec<ModularShaftSpec>()) // The Power Shaft, will crash if modified
        {
            return false;
        }

        if (originalTemplateSpec.GetSpec<BuildingSpec>() is not { } bld) { return false; }
        if (bld.PlaceFinished) // Free buildings are skipped unless they are district centers
        {
            return bld.HasSpec<DistrictCenterSpec>();
        }

        return true;
    }

    static void AddTransput(List<TransputSpec> transputs, Vector3Int coords, Directions3D directions, bool reverseRotation)
    {
        if (directions == Directions3D.None) { return; }

        transputs.Add(new()
        {
            Coordinates = coords,
            ReverseRotation = reverseRotation,
            Directions = directions,
        });
    }

    static Directions3D FindDirections(Vector3Int coords, Dictionary<Vector3Int, bool> blocks)
    {
        Directions3D result = Directions3D.None;

        foreach (var n in Deltas.Neighbors6Vector3Int)
        {
            var target = coords + n;
            if (!blocks.ContainsKey(target))
            {
                var d = Direction3DExtensions.FromOffset(n);
                result |= d.ToDirections3D();
            }
        }

        return result;
    }

    static IEnumerable<Direction3D> GetDirections(Directions3D directions)
    {
        foreach (var n in Deltas.Neighbors6Vector3Int)
        {
            var direction = Direction3DExtensions.FromOffset(n);
            if ((directions & direction.ToDirections3D()) != 0)
            {
                yield return direction;
            }
        }
    }

    IEnumerable<BuildingBlock> GetBuildingBlocks(BlockObjectSpec bo)
    {
        var (sx, sy, sz) = bo.Size;

        var occupiedBlocks = bo.GetBlocks().GetOccupiedBlocks()
            .Select(b => b.Coordinates)
            .ToHashSet();

        for (int x = 0; x < sx; x++)
        {
            var isXBorder = x == 0 || x == sx - 1;

            for (int y = 0; y < sy; y++)
            {
                var isYBorder = y == 0 || y == sy - 1;

                for (int z = 0; z < sz; z++)
                {
                    var coords = new Vector3Int(x, y, z);

                    if (!occupiedBlocks.Contains(coords)) { continue; }

                    var isBorder = isXBorder || isYBorder || z == 0 || z == sz - 1;
                    if (!isBorder) // Check for empty surrouding blocks
                    {
                        foreach (var n in Deltas.Neighbors6Vector3Int)
                        {
                            var target = coords + n;
                            if (!occupiedBlocks.Contains(target))
                            {
                                isBorder = true;
                                break;
                            }
                        }
                    }

                    yield return new BuildingBlock(coords, isBorder);
                }
            }
        }
    }

    readonly record struct BuildingBlock(Vector3Int Coords, bool IsBorder);
}
