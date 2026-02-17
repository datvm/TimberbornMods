namespace Timberborn.BaseComponentSystem;

public static partial class CommonTimberUiExtensions
{

    static readonly MethodInfo GetComponentMethod = typeof(BaseComponent).GetMethod(nameof(BaseComponent.GetComponent));
    static readonly Dictionary<Type, MethodInfo> getComponentCache = [];

    extension<T>(T component)
        where T : BaseComponent
    {

        public string GetTemplateName() => component.GetComponent<TemplateSpec>().TemplateName;

        public BaseComponent GetComponent(Type type)
        {
            var method = getComponentCache.GetOrAdd(type, () => GetComponentMethod.MakeGenericMethod(type));
            return (BaseComponent)method.Invoke(component, []);
        }

        public string GetName(ILoc t)
        {
            var namedEntity = component.GetComponent<NamedEntity>();
            if (namedEntity) { return namedEntity.EntityName; }

            return component.GetLabeledName(t);
        }

        public string GetLabeledName(ILoc t)
        {
            var comp = component.GetComponent<LabeledEntity>();
            if (comp) { return comp.DisplayName; }

            var spec = component.GetComponent<LabeledEntitySpec>();
            if (spec is not null) { return t.T(spec.DisplayNameLocKey); }

            var namedEntity = component.GetComponent<NamedEntity>();
            if (namedEntity) { return namedEntity.EntityName; }

            throw new InvalidOperationException($"Component {component.Name} does not have a name or label.");
        }

    }

}
