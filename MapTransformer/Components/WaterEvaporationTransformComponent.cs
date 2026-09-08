namespace MapTransformer.Components;

[MultiBind(typeof(IMapResizeComponent), Contexts = BindAttributeContext.NonMenu)]
class WaterEvaporationTransformComponent(
    WaterEvaporationMap waterEvaporationMap,
    WaterSimulator waterSimulator,
    MapIndexService mapIndexService
) : IMapResizeComponent
{
    public int Order => 40;

    public void Transform(in MapTransformContext ctx)
    {
        var oldVs = ctx.OldMapIndex.VerticalStride;
        var oldValues = waterEvaporationMap._evaporationModifiers.Current.ToArray();
        var oldMax = oldVs == 0 ? 1 : Math.Max(1, oldValues.Length / oldVs);

        waterEvaporationMap.Resize(Math.Max(1, waterSimulator.MaxColumnCount));
        var newValues = waterEvaporationMap._evaporationModifiers.Current.AsSpan();
        var newMax = Math.Max(1, waterSimulator.MaxColumnCount);
        MapIndexCopy.CopyColumnFloats(
            oldValues,
            oldVs,
            oldMax,
            newValues,
            mapIndexService.VerticalStride,
            newMax,
            in ctx);
        waterEvaporationMap._evaporationModifiers.Unify();
    }
}
