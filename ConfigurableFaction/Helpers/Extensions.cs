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

}
