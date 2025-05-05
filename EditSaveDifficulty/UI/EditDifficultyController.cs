global using Timberborn.GameWonderCompletion;

namespace EditSaveDifficulty.UI;

public class EditDifficultyController(
    IOptionsBox iOptionsBox,
    ILoc t,
    IContainer container,
    PanelStack panelStack,
    FactionService factions,
    MapNameService mapNames,
    NewGameParameterService parameters,
    DialogBoxShower diagShower
) : ILoadableSingleton
{
    readonly GameOptionsBox optionsBox = (GameOptionsBox)iOptionsBox;

    public void Load()
    {
        var root = optionsBox._root;
        
        var btnSave = root.Q("SaveGameButton");

        var btn = root.AddMenuButton(
            "LV.ESD.ChangeDifficulty".T(t),
            onClick: ShowChangeDiffDialogAsync,
            stretched: true);
        btn.InsertSelfBefore(btnSave);
    }

    async void ShowChangeDiffDialogAsync()
    {
        var diag = container.GetInstance<EditDifficultyDialog>();

        diag.SetContent(
            factions.Current, 
            mapNames.Name,
            parameters.GatherCurrentParameters());

        if (!await diag.ShowAsync(null, panelStack)) { return; }

        var newGameMode = diag.NewGameMode;
        parameters.SetNewParameters(newGameMode);

        diagShower.Create()
            .SetMessage("LV.ESD.Changed".T(t))
            .Show();
    }

}
