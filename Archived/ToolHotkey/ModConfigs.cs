global using Timberborn.KeyBindingSystem;
global using ToolHotkey.Services;
global using Timberborn.BlockSystem;
global using Timberborn.InputSystem;
global using Timberborn.PrefabSystem;

namespace ToolHotkey;

[Context("Game")]
public class GameConfig : Configurator
{

    public override void Configure()
    {
        Bind<ToolSelector>().AsSingleton();
    }

}

public class ModStarter : IModStarter
{
    public const string KeyGroupId = "Tools";
    public const string ToolKeyId = "Tool.{0}";

    public static string ModFolder { get; private set; } = null!;

    public void StartMod(IModEnvironment modEnvironment)
    {
        ModFolder = modEnvironment.ModPath;

        var harmony = new Harmony(nameof(ToolHotkey));
        harmony.PatchAll();
    }

}
