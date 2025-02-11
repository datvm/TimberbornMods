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

        container.Add(CreateToggleAllButton("DAM.EnableAll", true));
        container.Add(CreateToggleAllButton("DAM.DisableAll", false));
        container.Add(CreateButton("DAM.SaveProfile", SaveProfileMods, "DAM.SaveProfileDesc"));
        container.Add(CreateButton("DAM.LoadProfile", LoadProfileMods, "DAM.LoadProfileDesc"));
        container.Add(CreateButton("DAM.ViewProfileFolder", OpenProfileFolder, "DAM.ViewProfileFolderDesc"));
    }

    Button CreateButton(string key, Action action, string? tooltipKey = default)
    {
        return new(action)
        {
            text = loc.T(key),
            tooltip = tooltipKey is null ? null : loc.T(key),
            style = { color = new(Color.white), },
        };
    }

    Button CreateToggleAllButton(string key, bool enabled, string? tooltipKey = default)
    {
        return CreateButton(key,
            () => ToggleMods(enabled),
            tooltipKey);
    }

    void LoadProfileMods()
    {
        var profilePath = GetProfilePath();
        if (!File.Exists(profilePath))
        {
            Debug.LogError($"Profile file not found: {profilePath}");
            return;
        }

        var ids = File.ReadAllLines(profilePath)
            .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var m in diag._modListView._modItems.Values)
        {
            ToggleModItem(m, ids.Contains(m.Mod.Manifest.Id));
        }
    }

    void SaveProfileMods()
    {
        var profilePath = GetProfilePath();
        var ids = diag._modListView._modItems.Values
            .Where(m => m.Mod.IsEnabled)
            .Select(m => m.Mod.Manifest.Id);
        File.WriteAllLines(profilePath, ids);
    }

    void OpenProfileFolder() => System.Diagnostics.Process.Start(ModStarter.ModPath);

    static string GetProfilePath() => Path.Combine(ModStarter.ModPath, "mods.txt");

    void ToggleMods(bool enabled)
    {
        HashSet<string> ignoredList = enabled ? [] : s.KeepEnabledIds;

        foreach (var m in diag._modListView._modItems.Values)
        {
            if (ignoredList.Contains(m.Mod.Manifest.Id)) { continue; }

            ToggleModItem(m, enabled);
        }
    }

    void ToggleModItem(ModItem m, bool enabled)
    {
        m.ToggleMod(enabled);

        var toggle = m.Root.Q<Toggle>("ModToggle");
        toggle.value = enabled;
    }

}
