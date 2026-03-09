
namespace Timberborn.BaseComponentSystem;

public static class ConversionHelper
{

    extension(BaseComponent comp)
    {
        public Guid GetEntityId() => comp.GetComponent<EntityComponent>().EntityId;
    }

    extension(EntityComponent comp)
    {
        public HttpEntityModel Http() => new(comp.EntityId, (HttpEntityState)comp._entityState);
    }

    extension(LabeledEntity comp)
    {
        public HttpLabeledEntity Http() => new(comp.DisplayName, comp._labeledEntitySpec.Icon?.Path);
    }

    extension(NamedEntity comp)
    {
        public HttpNamedEntity Http() => new(comp.EntityName);
    }

    extension(Versioning.Version version)
    {
        public HttpGameVersion Http() => new(version.Full);
    }

}
