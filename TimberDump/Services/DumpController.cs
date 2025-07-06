
namespace TimberDump.Services;

public class DumpController(
    MainMenuPanel mainMenuPanel,
    DumpService dumpService,
    TextureDumper textureDumper
) : ILoadableSingleton
{

    public void Load()
    {
        var btnDumpData = CreateButton("Dump Data", dumpService.Dump);
        var btnDumpImage = CreateButton("Dump Images", textureDumper.Dump);

        var parent = mainMenuPanel._root.Q("ExitButton").parent;        
        parent.Insert(0, btnDumpData);
        parent.Insert(1, btnDumpImage);
    }

    Button CreateButton(string text, Action action)
    {
        var btn = new NineSliceButton()
        {
            text = text
        };
        btn.classList.Add("menu-button");
        btn.classList.Add("menu-button--stretched");
        btn.clicked += action;
        return btn;
    }

}
