namespace Bindito.Core;

public static class MoreHttpApiExtensions
{

    extension(Configurator config)
    {
        public Configurator BindMoreHttpApiHandlers(Assembly? assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass && !type.IsAbstract && typeof(IMoreHttpApiHandler).IsAssignableFrom(type))
                {
                    config.BindSingleton(type);
                    config.MultiBind(typeof(IMoreHttpApiHandler), type, true);
                }
            }

            return config;
        }
    }

}
