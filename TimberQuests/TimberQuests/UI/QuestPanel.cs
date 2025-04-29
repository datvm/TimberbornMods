namespace TimberQuests.UI;

public class QuestPanel(
    ILoc t,
    VisualElementInitializer veInit
) : ILoadableSingleton
{

#nullable disable
    public VisualElement Root { get; private set; }
    ScrollView content;
    Label lblNoQuest;
#nullable enable

    public void Load()
    {
        InitializeRoot();
        Root.Initialize(veInit);
    }

    void InitializeRoot()
    {
        Root = new NineSliceVisualElement()
            .AddClass("square-large--green")
            .SetPadding(10);
        Root.style.width = 260;
        Root.style.maxHeight = 500;

        Root.AddGameLabel("LV.TQ.QuestsTitle".T(t).Bold(), color: UiBuilder.GameLabelColor.Yellow)
            .SetMarginBottom(10);
        lblNoQuest = Root.AddGameLabel("LV.TQ.NoQuest".T(t));

        content = Root.AddScrollView();
    }

}
