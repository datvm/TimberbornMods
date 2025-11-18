namespace EditSaveDifficulty.UI;

public class EditDifficultyDialog(
    VisualElementLoader veLoader,
    PanelStack panelStack,
    ILoc t,
    CustomNewGameModeController customNewGameModeController,
    GameModeSpecService gameModeSpecService
) : DialogBoxElement
{
    static readonly ImmutableArray<string> InvisibleNames = [
        "Modes", "BackButton", "NextButton", 
        "StartingAdultsWrapper", "StartingChildrenWrapper",
        "StartingFoodWrapper", "StartingWaterWrapper"
    ];

    public GameModeSpec NewGameMode { get; private set; } = null!;

    public void SetContent(FactionSpec factionSpec, string mapName, GameModeSpec current)
    {
        SetTitle("LV.ESD.ChangeDifficulty".T(t));
        AddCloseButton();
        SetDialogPercentSize(.95f);

        var warningContent = "LV.ESD.SaveWarning".T(t).Bold().Color(TimberbornTextColor.Red);
        Content.AddGameLabel(warningContent, centered: true);

        var newGamePanel = new NewGameModePanel(veLoader, null, panelStack, t, customNewGameModeController, gameModeSpecService);
        newGamePanel.Load();

        newGamePanel.SelectFactionAndMap(
            factionSpec,
            // only map name is accessed:
            new(default, mapName, null, null, false, false, false, false, null));

        newGamePanel._predefinedGameMode = NewGameMode = current;
        newGamePanel.OnCustomizeButtonClicked();

        var root = newGamePanel.GetPanel();
        foreach (var name in InvisibleNames)
        {
            root.Q(name)?.SetDisplay(false);
        }

        Content.Add(root.parent);

        Content.AddGameLabel(warningContent, centered: true);
        Content.AddMenuButton("LV.ESD.ChangeDifficulty".T(t), 
            onClick: OnConfirm,
            size: UiBuilder.GameButtonSize.Large,
            stretched: true);

        style.maxWidth = new Length(100, LengthUnit.Percent);
        style.maxHeight = new Length(100, LengthUnit.Percent);
    }

    void OnConfirm()
    {
        if (!customNewGameModeController.TryGetValidatedGameMode(out var newGameMode))
        {
            return;
        }

        NewGameMode = newGameMode;
        OnUIConfirmed();
    }

}
