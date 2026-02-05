namespace PowerCopy.Services;

public static class PowerCopyExtensions
{
    static readonly MethodInfo GetComponentMethod = typeof(BaseComponent).GetMethod(nameof(BaseComponent.GetComponent));
    static readonly Dictionary<Type, MethodInfo> GetComponentMethodCache = [];

    extension(Type type)
    {
        public bool IsValidDuplicableType() => PowerCopyService.IsValidDuplicableType(type);
    }

    extension<T>(T comp) where T : BaseComponent
    {

        public BaseComponent? GetComponent(Type t)
        {
            var m = GetComponentMethodCache.GetOrAdd(t, () => GetComponentMethod.MakeGenericMethod(t));
            return (BaseComponent)m.Invoke(comp, []);
        }

        public string GetTemplateName() => comp.GetComponent<TemplateSpec>().TemplateName;

    }

}
