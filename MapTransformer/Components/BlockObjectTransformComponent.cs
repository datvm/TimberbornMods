namespace MapTransformer.Components;

[MultiBind(typeof(IMapResizeComponent), Contexts = BindAttributeContext.NonMenu)]
class BlockObjectTransformComponent(EntityRegistry entities) : IMapResizeComponent
{
    public int Order => 80;

    public void Transform(in MapTransformContext ctx)
    {
        foreach (var entity in entities.Entities.ToArray())
        {
            var obj = entity.GetComponent<BlockObject>();
            if (!obj)
            {
                continue;
            }

            if (!ctx.Transform.TryMapPlacement(obj.Placement, obj.Blocks.Size, out var dest))
            {
                continue;
            }

            obj.UpdateValues(dest);
        }
    }
}
