namespace TimberQuests;

[Context("MainMenu")]
public class ModMainMenuConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }
}

[Context("Game")]
public class ModGameConfig : Configurator
{

    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();

        Bind<TimberQuestRegistry>().AsSingleton();
        Bind<TimberQuestTracker>().AsSingleton();
        Bind<TimberQuestService>().AsSingleton();
        Bind<TimberQuestsUpdater>().AsSingleton();

        ConfigureUi();
        ConfigureTestCode();
    }

    void ConfigureUi()
    {
        Bind<QuestUIController>().AsSingleton();
        Bind<QuestPanel>().AsSingleton();
    }

    void ConfigureTestCode()
    {
#if TESTINGMOD
        Bind<TestCode.TutorialLumberQuestManager>().AsSingleton();
#endif
    }

}
