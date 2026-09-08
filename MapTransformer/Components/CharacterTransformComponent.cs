namespace MapTransformer.Components;

[MultiBind(typeof(IMapResizeComponent), Contexts = BindAttributeContext.NonMenu)]
class CharacterTransformComponent(EntityRegistry entities) : IMapResizeComponent
{
    public int Order => 90;

    public void Transform(in MapTransformContext ctx)
    {
        foreach (var entity in entities.Entities)
        {
            var character = entity.GetComponent<Character>();
            if (!character)
            {
                continue;
            }

            var mapped = ctx.Transform.MapWorld(character.Transform.position);
            character.Transform.position = mapped;
            var model = character.GetComponent<CharacterModel>();
            if (model)
            {
                model.Position = mapped;
            }

            var walker = character.GetComponent<Walker>();
            if (walker)
            {
                walker.StopNextTick();
            }
        }
    }
}
