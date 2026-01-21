namespace TImprove4Ui.Patches;

[HarmonyPatch(typeof(SettlementNameBox))]
public static class SettlementNameBoxPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(SettlementNameBox.GetPanel))]
    public static void AddBackToMenuOption(SettlementNameBox __instance)
    {
        const string name = "ExitToMenuButton";

        var root = __instance._root;
        var btn = root.Q(name);
        if (btn is not null) { return; }

        var lbl = root.Q("Message");
        var container = lbl.AddRow()
            .SetWidthPercent(100)
            .SetMarginBottom()
            .InsertSelfBefore(lbl);

        btn = container.AddButton(
            __instance._dialogBoxShower._loc.T("LV.T4UI.ExitToMenu"),
            onClick: OnExitToMenu,
            name: name,
            style: UiBuilder.GameButtonStyle.Text
        )
            .SetMarginLeftAuto()
            .Initialize(__instance._dialogBoxShower._visualElementLoader._visualElementInitializer);
    }

    public static void OnExitToMenu()
    {
        if (StaticSingletonsService.SceneLoader is null) { return; }

        StaticSingletonsService.SceneLoader.LoadSceneInstantly(MainMenuSceneParameters.CreateWithoutWelcomeScreen());
    }

}
