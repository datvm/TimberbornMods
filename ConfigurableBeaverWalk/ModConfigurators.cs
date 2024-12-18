using Bindito.Core;
using HarmonyLib;

namespace ConfigurableBeaverWalk
{

    [Context("MainMenu")]
    public class ModConfigurators : IConfigurator
    {
        static bool patched = false;

        public void Configure(IContainerDefinition containerDefinition)
        {
            if (!patched)
            {
                patched = true;
                new Harmony(nameof(ConfigurableBeaverWalk)).PatchAll();
            }

            containerDefinition.Bind<ModSettings>().AsSingleton();
        }
    }

    [Context("Game")]
    public class ModGameConfigurators : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<ModSettings>().AsSingleton();
        }
    }

}
