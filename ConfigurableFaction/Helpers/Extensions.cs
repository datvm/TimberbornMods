namespace ConfigurableFaction.Helpers;

public static class Extensions
{

    extension(Blueprint bp)
    {
        public TDef? CreateDefinition<TDef, T>(Func<T, TDef> createFunc)
            where TDef : TemplateDefBase
            where T : ComponentSpec
        {
            var comp = bp.GetSpec<T>();
            return comp is null ? null : createFunc(comp);
        }
    }

    extension<T>(HashSet<T> set)
    {
        public void Toggle(T item, bool value)
        {
            if (value)
            {
                set.Add(item);
            }
            else
            {
                set.Remove(item);
            }
        }
    }

}
