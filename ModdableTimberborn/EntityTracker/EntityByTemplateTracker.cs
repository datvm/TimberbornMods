namespace ModdableTimberborn.EntityTracker;

public class EntityByTemplateTracker : IEntityTracker<TemplateTrackerComponent>
{
    readonly HashSet<TemplateTrackerComponent> entities = [];
    public IReadOnlyCollection<TemplateTrackerComponent> Entities => entities;

    readonly Dictionary<string, HashSet<TemplateTrackerComponent>> entitiesByTemplate = [];
    public IReadOnlyCollection<string> TrackedTemplates => entitiesByTemplate.Keys;

    public event Action<TemplateTrackerComponent>? OnEntityRegistered;
    public event Action<TemplateTrackerComponent>? OnEntityUnregistered;

    public IReadOnlyCollection<TemplateTrackerComponent> GetEntitiesByTemplate(string templateName) 
        => entitiesByTemplate.TryGetValue(templateName, out var entities) ? entities : [];

    public void Track(EntityComponent entity)
    {
        var template = entity.GetComponent<TemplateTrackerComponent>();
        if (!template) { return; }

        entitiesByTemplate.GetOrAdd(template.TemplateName, () => []).Add(template);
        if (entities.Add(template))
        {
            OnEntityRegistered?.Invoke(template);
        }
    }

    public void Untrack(EntityComponent entity)
    {
        var template = entity.GetComponent<TemplateTrackerComponent>();
        if (!template) { return; }

        if (entitiesByTemplate.TryGetValue(template.TemplateName, out var entities))
        {
            entities.Remove(template);
            if (entities.Count == 0)
            {
                entitiesByTemplate.Remove(template.TemplateName);
            }
        }

        if (entities.Remove(template))
        {
            OnEntityUnregistered?.Invoke(template);
        }
    }
}
