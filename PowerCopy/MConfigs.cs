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
            .BindSingleton<PowerCopyService>()
            .BindSingleton<ObjectListingService>()
            .BindSingleton<PowerCopyAreaTool>()

            .BindSingleton<DuplicableEntryFactory>()
            .BindOrderedFragment<PowerCopyFragment>()
            
            .BindTransient<ObjectSelectionDialog>();  
    }

}
