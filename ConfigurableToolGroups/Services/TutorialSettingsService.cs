namespace ConfigurableToolGroups.Services;

public class TutorialSettingsService(TutorialSettings tutorialSettings) : ILoadableSingleton
{

    public static bool TutorialsDisabled { get; private set; }

    public void Load()
    {
        TutorialsDisabled = tutorialSettings.DisableTutorial;
    }
}
