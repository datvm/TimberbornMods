namespace WeatherEditor.UI;

public class WeatherEditorController(
    IOptionsBox iOptionsBox,
    IContainer container,
    ILoc t,
    PanelStack panelStack,
    GameSaverService saver,
    DialogBoxShower diag
) : ILoadableSingleton
{

    readonly GameOptionsBox optionsBox = (GameOptionsBox)iOptionsBox;

    public void Load()
    {
        var root = optionsBox._root;

        var btnResume = root.Q("ResumeButton");

        var btn = root.AddMenuButton(t.T("LV.WEdit.WeatherEditor"), onClick: ShowDialog, stretched: true);
        btn.InsertSelfAfter(btnResume);
    }

    async void ShowDialog()
    {
        var diag = container.GetInstance<WeatherEditorDialog>();
        var confirm = await diag.ShowAsync(initializer: null, panelStack);

        if (!confirm) { return; }
        await SaveAndLoadAsync();
    }

    async Task SaveAndLoadAsync()
    {
        var r = await saver.SaveAsync();

        var confirm = await diag.ShowAsync("LV.WEdit.AskToLoad",
            buttons: [null, null],
            messageLocalized: true);
        if (confirm != true) { return; }

        saver.Load(r);
    }

}
