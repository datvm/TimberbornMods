namespace TimberUi.Helpers;

public class TemplateModuleHelper(Configurator configurator)
{

    readonly TemplateModule.Builder builder = new();

    public TemplateModuleHelper AddDecorator<TSubject, TDecorator>(bool addTransient = true)
        where TDecorator : class
    {
        builder.AddDecorator<TSubject, TDecorator>();

        if (addTransient)
        {
            configurator.BindTransient<TDecorator>();
        }

        return this;
    }

    public void Bind()
    {
        configurator.MultiBind<TemplateModule>().ToProvider(builder.Build).AsSingleton();
    }

}

public class MassRebindingHelper(Configurator configurator)
{

    readonly List<KeyValuePair<Type, Type>> replacings = [];
    readonly List<Type> removing = [];
    readonly HashSet<Type> bindings = [];

    public MassRebindingHelper Replace<TRemove, TReplace>(bool alsoBindReplacement = true)
        where TReplace : TRemove
    {
        replacings.Add(new(typeof(TRemove), typeof(TReplace)));
        if (alsoBindReplacement)
        {
            bindings.Add(typeof(TReplace));
        }

        return this;
    }

    public MassRebindingHelper Remove<T>() where T : class
    {
        removing.Add(typeof(T));
        return this;
    }

    public Configurator Bind()
    {
        // First remove alls bindings
        configurator.MassRemoveBindings(replacings
            .Select(q => q.Key)
            .Concat(removing));

        // Then re-add replacements
        foreach (var (src, dst) in replacings)
        {
            configurator.BindSingleton(src, dst, bindings.Contains(dst));
        }

        return configurator;
    }

}