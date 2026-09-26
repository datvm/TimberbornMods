namespace ConveyorBelt.Services;

[BindSingleton]
public class RiserPlacer(BlockObjectSpawningHelper spawner)
{
    public bool TryPlaceAbove(BlockObject source, string speedId, BeltShape shape)
    {
        if (shape is not (BeltShape.RiserUp or BeltShape.RiserDown))
        {
            return false;
        }

        var dir = shape == BeltShape.RiserDown ? "Down" : "Up";
        var template = $"ConveyorRiser{speedId}{dir}.Folktails";
        if (!spawner.TryGetBlockObjectSpec(template, out var spec) || spec is null)
        {
            return false;
        }

        var placement = new Placement(source.Coordinates + Direction3D.Top.ToOffset(), source.Orientation, source.FlipMode);
        if (!spawner.IsPlacementValid(spec, placement))
        {
            return false;
        }

        spawner.PlaceObject(spec, placement);
        return true;
    }
}
