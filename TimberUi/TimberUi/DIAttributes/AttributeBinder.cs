namespace TimberUi;

public static class AttributeBinder
{
    readonly record struct DecoratorInfo(Type Subject, Type Decorator, bool BindTransient);

    public static void BindAttributes(Assembly assembly, Configurator configurator, Scope defaultScope, BindAttributeContext context)
    {
        var isNonMenu = (context & BindAttributeContext.NonMenu) > 0;
        var isGame = (context & BindAttributeContext.Game) > 0;

        List<KeyValuePair<Type, Type>> decorators = [];

        foreach (Type type in assembly.GetTypes())
        {
            BindType(type, configurator, defaultScope, context);

            if (isNonMenu)
            {
                BindEntityFragment(type, configurator);

                if (isGame)
                {
                    foreach (var attr in type.GetCustomAttributes<AddTemplateModuleAttribute>())
                    {
                        decorators.Add(new(attr.Subject, type));

                        if (attr.AlsoBindTransient)
                        {
                            configurator.Bind(type).AsTransient();
                        }
                    }
                }

                foreach (var attr in type.GetCustomAttributes<AddTemplateModule2Attribute>())
                {
                    if ((attr.Contexts & context) == 0) { continue; }

                    decorators.Add(new(attr.Subject, type));

                    if (attr.AlsoBindTransient)
                    {
                        configurator.Bind(type).AsTransient();
                    }
                }
            }
        }

        if (decorators.Count > 0)
        {
            BindTemplateModule(decorators, configurator);
        }
    }

    static void BindType(Type type, Configurator configurator, Scope defaultScope, BindAttributeContext context)
    {
        var bindAttrs = type.GetCustomAttributes<BindAttribute>();

        foreach (var attr in bindAttrs)
        {
            if ((attr.Contexts & context) == 0) { continue; }

            var scope = attr.Scope ?? defaultScope;
            var asType = attr.As;
            var alsoBindSelf = attr.AlsoBindSelf;

            if (alsoBindSelf && asType is null)
            {
                throw new InvalidOperationException($"With {nameof(attr.AlsoBindSelf)}, attribute on type {type.FullName} must specify an '{nameof(attr.As)}' type.");
            }

            if (attr.MultiBind)
            {
                if (asType is null)
                {
                    throw new InvalidOperationException($"Multibind attribute on type {type.FullName} must specify an '{nameof(attr.As)}' type.");
                }

                if (alsoBindSelf)
                {
                    configurator.Bind(type).AsScope(scope);
                    configurator.MultiBind(asType, type, true);
                }
                else
                {
                    configurator.MultiBind(asType, type, false).AsScope(scope);
                }
            }
            else
            {
                if (asType is null)
                {
                    configurator.Bind(type).AsScope(scope);
                }
                else
                {
                    configurator.Bind(asType, type, scope, alsoBindSelf);
                }
            }
        }
    }

    static void BindEntityFragment(Type type, Configurator configurator)
    {
        if (type.GetCustomAttribute<BindFragmentAttribute>() is not null)
        {
            if (!type.IsEntityPanelFragment())
            {
                throw new InvalidOperationException($"Type {type.FullName} is marked with {nameof(BindFragmentAttribute)} but does not implement {nameof(IEntityPanelFragment)}.");
            }

            if (type.IsEntityFragmentOrder())
            {
                configurator.BindOrderedFragment(type);
            }
            else
            {
                configurator.BindFragment(type);
            }
        }
    }

    static void BindTemplateModule(List<KeyValuePair<Type, Type>> decorators, Configurator configurator)
    {
        configurator.BindTemplateModule(h =>
        {
            foreach (var (sub, dec) in decorators)
            {
                h.AddDecorator(sub, dec, false);
            }

            return h;
        });
    }

}
