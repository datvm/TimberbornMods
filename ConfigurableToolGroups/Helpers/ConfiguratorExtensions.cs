namespace Bindito.Core;

public static class ConfiguratorExtensions
{

    extension(Configurator config)
    {

        Configurator MultiBindSingleton<T, TImpl>(bool alsoBindSelf)
            where T : class
            where TImpl : class, T
        {
            if (alsoBindSelf)
            {
                config.Bind<TImpl>().AsSingleton();
                config.MultiBind<T>().ToExisting<TImpl>();
            }
            else
            {
                config.MultiBind<T>().To<TImpl>().AsSingleton();
            }
            return config;
        }

        public Configurator MultiBindCustomTool<TElement>(bool alsoBindSelf = false)
            where TElement : CustomBottomBarElement
            => config.MultiBindSingleton<CustomBottomBarElement, TElement>(alsoBindSelf);

        public Configurator MultiBindElementsRemover<TRemove>()
            where TRemove : class, IBottomBarElementsRemover
            => config.MultiBindSingleton<IBottomBarElementsRemover, TRemove>(false);

    }

}
