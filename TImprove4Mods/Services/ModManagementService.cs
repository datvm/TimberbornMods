global using Timberborn.ModdingUI;

namespace TImprove4Mods.Services;

public class ModFilterParameters
{
    public string Keyword { get; set; } = "";
    public bool Enabled { get; set; } = true;
    public bool Disabled { get; set; } = true;
}

public class ModManagementService(
    DialogBoxShower diagShower,
    ModManagerBox modManagerBox,
    FileDialogService fileDialogs
)
{

    readonly ModFilterParameters filters = new();

    public void ToggleAll(bool enabled)
    {
        diagShower.Create()
            .SetLocalizedMessage("LV.T4Mods." + (enabled ? "ConfirmEnableAll" : "ConfirmDisableAll"))
            .SetConfirmButton(() => PerformToggleAll(enabled))
            .SetDefaultCancelButton()
            .Show();
    }

    void PerformToggleAll(bool enabled)
    {
        foreach (var mod in ModItems)
        {
            ToggleModItem(mod, enabled);
        }
    }

    public void RestartGame()
    {
        var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        System.Diagnostics.Process.Start(exePath);

        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }

    public void SaveProfile()
    {
        var filePath = fileDialogs.SaveFile();
        if (filePath is null) { return; }

        if (Path.GetFileName(filePath).IndexOf('.') == -1) { filePath += ".txt"; }

        var ids = ModItems
            .Where(q => ModPlayerPrefsHelper.IsModEnabled(q.Mod)) // Don't use IsEnabled, it's not updated
            .Select(q => q.Mod.Manifest.Id);

        File.WriteAllLines(filePath, ids);
    }

    public void LoadProfile()
    {
        var filePath = fileDialogs.OpenFile();
        if (filePath is null || !File.Exists(filePath)) { return; }

        var ids = File.ReadAllLines(filePath)
            .Select(q => q.Trim())
            .Where(q => !string.IsNullOrEmpty(q))
            .ToHashSet();

        foreach (var mod in ModItems)
        {
            var id = mod.Mod.Manifest.Id;

            ToggleModItem(mod, ids.Contains(id));
            ids.Remove(id); // Make sure only one is enabled, in case there are multiple from the same source
        }
    }

    public void Filter(string keyword)
    {
        filters.Keyword = keyword;
        UpdateFilterVisibility();
    }

    public void FilterState(bool state, bool enabled)
    {
        if (state)
        {
            filters.Enabled = enabled;
        }
        else
        {
            filters.Disabled = enabled;
        }

        UpdateFilterVisibility();
    }

    void UpdateFilterVisibility()
    {
        var kw = filters.Keyword;
        var hasKeyword = !string.IsNullOrEmpty(kw);

        foreach (var mod in ModItems)
        {
            var enabled = ModPlayerPrefsHelper.IsModEnabled(mod.Mod);

            if ((enabled && !filters.Enabled)
                || (!enabled) && !filters.Disabled)
            {
                mod.Root.SetDisplay(false);
            }
            else
            {
                mod.Root.SetDisplay(!hasKeyword || Match(mod, kw));
            }
        }
    }

    static bool Match(ModItem mod, string keyword)
    {
        return mod.Mod.Manifest.Id.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || mod.Mod.Manifest.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase);
    }

    static void ToggleModItem(ModItem m, bool enabled)
    {
        m.ToggleMod(enabled);

        var toggle = m.Root.Q<Toggle>("ModToggle");
        toggle.value = enabled;
    }

    IEnumerable<ModItem> ModItems => modManagerBox._modListView._modItems.Values;


}
