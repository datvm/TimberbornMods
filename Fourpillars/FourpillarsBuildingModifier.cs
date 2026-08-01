namespace Fourpillars;

[MultiBind(typeof(ITemplateModifier))]
public class FourpillarsBuildingModifier : ITemplateModifier
{
    public EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original)
    {
        Vector3? pivot = null;

        template.TransformSpec<BlockObjectSpec>(bos =>
        {
            var blocks = bos.Blocks.ToArray();
            var (sx, sy, _) = bos.Size;
            var baseZ = bos.BaseZ;

            List<Vector2Int> support = [];
            for (int y = 0; y < sy; y++)
            {
                for (int x = 0; x < sx; x++)
                {
                    var index = BlockIndex(x, y, baseZ, sx, sy);
                    if (blocks[index].MatterBelow.IsSolidMatter())
                    {
                        support.Add(new(x, y));
                    }
                }
            }

            if (support.Count <= 4) { return null; }

            var (sw, nw, se, ne) = SelectCornerSupports(support);
            pivot = new(sw.x + 0.5f, sw.y + 0.5f, 0f);

            HashSet<Vector2Int> keep = [sw, nw, se, ne];
            foreach (var cell in support)
            {
                if (keep.Contains(cell)) { continue; }

                var index = BlockIndex(cell.x, cell.y, baseZ, sx, sy);
                blocks[index] = blocks[index] with { MatterBelow = MatterBelow.Any };
            }

            return bos with { Blocks = [.. blocks] };
        });

        if (pivot is not { } pivotCoords) { return null; } // No change

        template.TransformSpec<PlaceableBlockObjectSpec>(pbos => pbos with
        {
            CustomPivot = new()
            {
                HasCustomPivot = true,
                Coordinates = pivotCoords,
            },
        });

        return template;
    }

    public bool ShouldModify(string blueprintName, string templateName, TemplateSpec org)
        // Only apply to buildings placed by the user; skip understructure / underground specials.
        => org.GetSpec<BlockObjectSpec>() is { } bld
        && !org.HasSpec<UnderstructureConstraintSpec>()
        && org.HasSpec<BuildingSpec>()
        && org.HasSpec<PlaceableBlockObjectSpec>()
        // Smaller than or equal to 2x2 already has at most four footprint cells.
        && bld.Size is var (x, y, _)
        && (x > 2 || y > 2)
        && !bld.Blocks.FastAny(b => b.Underground);

    /// <summary>
    /// Four diagonal extremes of the support set (ties broken on the secondary axis).
    /// SW: min x then min y; NW: min x then max y; SE: max x then min y; NE: max x then max y.
    /// </summary>
    static (Vector2Int Sw, Vector2Int Nw, Vector2Int Se, Vector2Int Ne) SelectCornerSupports(List<Vector2Int> support)
    {
        var sw = support[0];
        var nw = support[0];
        var se = support[0];
        var ne = support[0];

        foreach (var cell in support)
        {
            if (cell.x < sw.x || (cell.x == sw.x && cell.y < sw.y)) { sw = cell; }
            if (cell.x < nw.x || (cell.x == nw.x && cell.y > nw.y)) { nw = cell; }
            if (cell.x > se.x || (cell.x == se.x && cell.y < se.y)) { se = cell; }
            if (cell.x > ne.x || (cell.x == ne.x && cell.y > ne.y)) { ne = cell; }
        }

        return (sw, nw, se, ne);
    }

    static int BlockIndex(int x, int y, int z, int sizeX, int sizeY)
        => (z * sizeY + y) * sizeX + x;
}
