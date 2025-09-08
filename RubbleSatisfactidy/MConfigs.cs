global using RubbleSatisfactidy.Services;

namespace RubbleSatisfactidy;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this.BindSingleton<RubbleCleanupService>();
    }
}
