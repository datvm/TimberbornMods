global using Timberborn.MainMenuModdingUI;
using Timberborn.ModdingUI;

namespace ToggleAllMods.UI;

public class ModManagerBoxUI(ModManagerBox diag, ILoc loc, ModSettings s) : IPostLoadableSingleton
{

    public void PostLoad()
    {
        var topButtons = diag._root.Q("TopButtons");
        var parent = topButtons.parent;

        var container = new VisualElement()
        {
            style =
            {
                display = DisplayStyle.Flex,
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.SpaceBetween,
                marginBottom = 10,
                marginTop = 10,
            }
        };
        parent.Insert(parent.IndexOf(topButtons) + 1, container);

        container.Add(CreateButton("DAM.EnableAll", EnableAllMods));
        container.Add(CreateButton("DAM.DisableAll", DisableAllMods));
        container.Add(CreateButton("DAM.SaveProfile", SaveProfileMods, "DAM.SaveProfileDesc"));
        container.Add(CreateButton("DAM.LoadProfile", LoadProfileMods, "DAM.LoadProfileDesc"));

        var restartWarning = diag._root.Q("RestartWarning");
        var restartBtn = CreateButton("DAM.RestartGame", RestartGame);
        restartBtn.style.marginTop = 30;

        restartWarning.parent.Add(restartBtn);
    }

    Button CreateButton(string key, Action action, string? tooltipKey = default)
    {
        Timberborn.CoreUI.LocalizableButton btn = new()
        {
            text = loc.T(key),
            tooltip = tooltipKey is null ? null : loc.T(key),
            style = { color = new(Color.white), },
        };
        btn.clicked += action;

        btn.classList.AddRange(["unity-text-element", "unity-button", "mod-manager-box__top-button", "text--default", "text--bold"]);

        return btn;
    }

    void RestartGame()
    {
        var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        System.Diagnostics.Process.Start(exePath);

        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }
    
    const string TextFilter = "Text Files (*.txt)\0*.txt\0\0";
    void LoadProfileMods()
    {
        var profilePath = WindowsFileDialogs.ShowOpenFileDialog(TextFilter, GetDefaultFolder());
        if (profilePath is null) { return; }

        var ids = File.ReadAllLines(profilePath)
            .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

        // Prioritize local mods first
        HashSet<string> enabledIds = [];

        foreach (var m in diag._modListView._modItems.Values)
        {
            if (!m.Mod.ModDirectory.IsUserMod) { continue; }

            var enabled = ids.Contains(m.Mod.Manifest.Id);
            ToggleModItem(m, enabled);

            if (enabled) { enabledIds.Add(m.Mod.Manifest.Id); }
        }

        // Then enable the rest
        foreach (var m in diag._modListView._modItems.Values)
        {
            if (m.Mod.ModDirectory.IsUserMod) { continue; }

            var enabled = ids.Contains(m.Mod.Manifest.Id) && !enabledIds.Contains(m.Mod.Manifest.Id);
            ToggleModItem(m, enabled);
        }
    }

    void SaveProfileMods()
    {
        var profilePath = WindowsFileDialogs.ShowSaveFileDialog(TextFilter, GetDefaultFolder());
        if (profilePath is null) { return; }

        if (Path.GetFileName(profilePath).IndexOf('.') == -1) { profilePath += ".txt"; }

        var ids = diag._modListView._modItems.Values
            .Where(m => m.Mod.IsEnabled)
            .Select(m => m.Mod.Manifest.Id);
        File.WriteAllLines(profilePath, ids);
    }

    void DisableAllMods()
    {
        var ignoredList = s.KeepEnabledIds;

        HashSet<string> keepingIds = [];
        HashSet<string> duplicateIds = [];
        foreach (var m in diag._modListView._modItems.Values)
        {
            var id = m.Mod.Manifest.Id;
            if (ignoredList.Contains(id))
            {
                if (keepingIds.Contains(id))
                {
                    duplicateIds.Add(id);
                }
                else
                {
                    keepingIds.Add(id);
                }
            }
            else
            {
                ToggleModItem(m, false);
            }
        }

        // Disable duplicates
        if (duplicateIds.Count > 0)
        {
            foreach (var m in diag._modListView._modItems.Values)
            {
                if (duplicateIds.Contains(m.Mod.Manifest.Id)
                    && !m.Mod.ModDirectory.IsUserMod)
                {
                    ToggleModItem(m, false);
                }
            }
        }
    }

    static string GetDefaultFolder() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Timberborn\PlayerData");

    void EnableAllMods()
    {
        foreach (var m in diag._modListView._modItems.Values)
        {
            ToggleModItem(m, true);
        }
    }

    void ToggleModItem(ModItem m, bool enabled)
    {
        m.ToggleMod(enabled);

        var toggle = m.Root.Q<Toggle>("ModToggle");
        toggle.value = enabled;
    }

}
