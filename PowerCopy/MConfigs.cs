global using PowerCopy.UI;
global using PowerCopy.Services;

namespace PowerCopy;

[Context(nameof(BindAttributeContext.Game))]
[Context(nameof(BindAttributeContext.MapEditor))]
public class MGameplayConfig : Configurator
{

    public override void Configure()
    {
        this
            .BindSingleton<ObjectListingService>()

            .BindSingleton<DuplicableEntryFactory>()
            .BindOrderedFragment<PowerCopyFragment>();  
    }

}
