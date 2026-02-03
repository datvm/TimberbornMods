global using PowerCopy.UI;

namespace PowerCopy;

[Context(nameof(BindAttributeContext.Game))]
[Context(nameof(BindAttributeContext.MapEditor))]
public class MGameplayConfig : Configurator
{

    public override void Configure()
    {
        this
            .BindSingleton<DuplicableEntryFactory>()
            .BindOrderedFragment<PowerCopyFragment>();  
    }

}
