namespace BuffDebuff;

public class BuffableService(IBuffEntityService buffEntities, EntityRegistry registry) : ILoadableSingleton, IBuffableService
{

    readonly Dictionary<long, BuffableComponent> buffables = [];

    public ICollection<BuffableComponent> Buffables => buffables.Values;
    public BuffableComponent this[long index] => buffables[index];

    public void Load()
    {
        foreach (var e in registry.Entities)
        {
            var buffable = e.GetComponentFast<BuffableComponent>()
                ?? throw new InvalidOperationException("Entity missing BuffableComponent");

            Register(buffable);
        }
    }

    public void Register(BuffableComponent c)
    {
        buffEntities.Register(c);
        buffables[c.Id] = c;
    }

    public void Unregister(BuffableComponent c)
    {
        buffEntities.Unregister(c);
        buffables.Remove(c.Id);
    }

}
