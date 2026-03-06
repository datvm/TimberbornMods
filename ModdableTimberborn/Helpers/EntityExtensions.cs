namespace ModdableTimberborn.Helpers;

public static class EntityExtensions
{


    extension<T>(T comp) where T : BaseComponent
    {
        public Guid GetEntityId() => comp.GetComponent<EntityComponent>().EntityId;
    }

    extension(EntityRegistry entityRegistry)
    {

        public EntityComponent? TryGetEntity(Guid? id) => id is null ? null : entityRegistry.GetEntity(id.Value);

        public string DescribeEntity(Guid? id, ILoc t)
        {
            var entity = entityRegistry.TryGetEntity(id);
            return entity ? entity!.GetName(t) : t.TNone();
        }

        public Automator? TryGetAutomator(Guid? id)
        {
            var entity = entityRegistry.TryGetEntity(id);
            if (!entity) { return null;  }

            var automator = entity!.GetComponent<Automator>();
            return automator ? automator : null;
        }

    }


}
