namespace MapTransformer.Transform;

interface IMapResizeComponent
{
    int Order { get; }
    void Transform(in MapTransformContext ctx);
}

readonly record struct MapTransformContext(
    MapTransform Transform,
    MapSize OldMapSize,
    MapIndexService OldMapIndex,
    MapSize MapSize,
    MapIndexService MapIndex);

static class Transforming
{
    public static bool Active { get; set; }
}
