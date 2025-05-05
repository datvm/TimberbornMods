global using EditSaveDifficulty.UI;
global using EditSaveDifficulty.Services;

namespace EditSaveDifficulty;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<NewGameParameterService>().AsSingleton();
        Bind<EditDifficultyDialog>().AsTransient();
        Bind<EditDifficultyController>().AsSingleton();

        Bind<CustomNewGameModeController>().AsSingleton();
    }
}
