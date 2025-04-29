namespace TimberQuests.UI;

public class QuestUIController(
    UILayout uiLayout,
    QuestPanel questPanel
) : ILoadableSingleton
{

    public void Load()
    {
        AppendUI();
    }

    void AppendUI()
    {
        uiLayout.AddBottomLeft(questPanel.Root, -100);
    }

}
