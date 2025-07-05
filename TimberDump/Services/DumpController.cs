namespace TimberDump.Services;

public class DumpController(
    MainMenuPanel mainMenuPanel,
    DumpService dumpService
) : ILoadableSingleton
{

    public void Load()
    {
        var btn = new NineSliceButton()
        {
            text = "Dump data"
        };
        btn.classList.Add("menu-button");
        btn.classList.Add("menu-button--stretched");

        btn.clicked += dumpService.Dump;

        var btnExit =  mainMenuPanel._root.Q("ExitButton");
        btnExit.parent.Insert(0, btn);
    }

}
