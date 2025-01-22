﻿using Bindito.Core;

namespace ToggleAllMods;
[Context("MainMenu")]
public class ModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<ModSettings>().AsSingleton();
    }
}
