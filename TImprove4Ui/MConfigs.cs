global using TImprove4Ui.Patches;
global using TImprove4Ui.Services;
global using System.Reflection.Emit;
global using Label = UnityEngine.UIElements.Label;

namespace TImprove4Ui;

[Context("MainMenu")]
public class MMenuConfig : MainMenuAttributeConfigurator;

[Context("Game")]
public class ModGameConfig : GameAttributeConfigurator;

[Context("MapEditor")]
public class MMapEditorConfig : MapEditorAttributeConfigurator;
