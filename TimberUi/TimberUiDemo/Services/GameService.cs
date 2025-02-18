namespace TimberUiDemo.Services;

public class GameService(UILayout layout) : ILoadableSingleton
{

    int counter;
    Label topBarLabel = null!;

    public void Load()
    {
        AddTopBarItem();
    }

    void AddTopBarItem()
    {
        var item = new NineSliceButton()
            .AddClasses([UiCssClasses.TopRightItemClass, UiCssClasses.ButtonTopBarPrefix + UiCssClasses.Green]);
        item.style.justifyContent = Justify.Center;

        item.clicked += OnTopBarItemClicked;

        topBarLabel = item.AddLabel("Hello TimberUi")
            .AddLabelClasses(GameLabelStyle.Game, color: GameLabelColor.Yellow);

        layout.AddTopRight(item, 7);
    }

    private void OnTopBarItemClicked()
    {
        topBarLabel.text = $"Clicked: {++counter} times.";
    }
}