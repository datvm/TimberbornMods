namespace TImprove4Modders.Patches;

[HarmonyPatch]
public static class ModPatches
{
    private const int BaseValue = 100_000_000;
    private const float MulValue = 10_000;

    readonly record struct ModWithPriority(Mod Mod, int CurrPriority);

    [HarmonyPrefix, HarmonyPatch(typeof(ModPlayerPrefsHelper), nameof(ModPlayerPrefsHelper.SetModPriority))]
    public static void PatchPriority(ref int priority)
    {
        var spacedOut = GetSpacedOutPriority(priority);
        priority = spacedOut;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ModSorter), nameof(ModSorter.Sort))]
    public static bool BetterSort(IEnumerable<Mod> mods, ref IEnumerable<Mod> __result)
    {
        mods = [.. mods];
        var count = mods.Count();
        if (!MSettings.BetterModOrder || count == 0) { return true; }

        var allZero = true;
        var isSpacedOut = true;

        var list = mods
            .Select((q, i) =>
            {
                var priority = ModPlayerPrefsHelper.GetModPriority(q);

                if (priority != 0)
                {
                    allZero = false;
                    if (priority < BaseValue) // This value was reset by the game
                    {
                        isSpacedOut = false;
                    }
                }

                return new ModWithPriority(q, priority);
            })
            .ToList();

        if (!isSpacedOut)
        {
            list = [.. list
                .Select((q, i) => {
                    ModPlayerPrefsHelper.SetModPriority(q.Mod, count - i);
                    return new ModWithPriority(q.Mod, GetSpacedOutPriority(count - i));
                })
                .OrderByDescending(q => q.CurrPriority)];

        }
        else
        {
            list = [.. list.OrderByDescending(q => q.CurrPriority)];
        }

        HashSet<string> availableMods = [];
        if (allZero)
        {
            // First, sort by enabled then name
            list = [.. list
                .OrderByDescending(q => ModPlayerPrefsHelper.IsModEnabled(q.Mod)) // Don't use Mod.IsEnabled, it's not updated
                .ThenBy(q => q.Mod.Manifest.Name)];

            // Then, scan one by one and bring the dependencies up
            for (int i = 0; i < list.Count; i++)
            {
                var (mod, _) = list[i];
                if (availableMods.Contains(mod.Manifest.Id)) { continue; } // It's already processed

                i = BringDependenciesUp(mod, i);
                availableMods.Add(mod.Manifest.Id);
            }

            for (int i = 0; i < list.Count; i++)
            {
                var (mod, currPriority) = list[i];
                ModPlayerPrefsHelper.SetModPriority(mod, count - i);
            }
        }

        __result = list.Select(q => q.Mod);
        return false;

        int BringDependenciesUp(Mod mod, int index)
        {
            foreach (var dep in mod.Manifest.RequiredMods)
            {
                if (availableMods.Contains(dep.Id)) { continue; }

                ModWithPriority modEntry = default;
                Mod? foundMod = null;
                int i;
                for (i = index + 1; i < list.Count; i++)
                {
                    modEntry = list[i];
                    foundMod = modEntry.Mod;
                    if (foundMod.Manifest.Id == dep.Id) { break; }
                }
                if (foundMod is null || foundMod.Manifest.Id != dep.Id) { continue; } // The dependency mod was not installed

                list.RemoveAt(i);
                list.Insert(index, modEntry);
                availableMods.Add(foundMod.Manifest.Id);

                index = BringDependenciesUp(foundMod, index) + 1;
            }

            return index;
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ModListView), nameof(ModListView.MoveToTop))]
    public static bool BetterMoveToTop(Mod mod, int index, VisualElement parent, ModListView __instance)
    {
        if (!MSettings.BetterModOrder) { return true; }

        var dependencies = mod.Manifest.RequiredMods.Select(q => q.Id).ToHashSet();

        var children = parent.Children();
        var count = children.Count();
        var counter = 0;
        var placed = false;

        for (int i = count - 1; i >= 0; i--)
        {
            if (index == i) { continue; } // Self

            var curr = __instance.GetModFromElement(parent.ElementAt(i));

            if (placed) // Depencies
            {
                ModPlayerPrefsHelper.SetModPriority(curr, ++counter);
            }
            else
            {
                if (dependencies.Contains(curr.Manifest.Id))
                {
                    // Place it here
                    ModPlayerPrefsHelper.SetModPriority(mod, ++counter);
                    placed = true;
                }

                ModPlayerPrefsHelper.SetModPriority(curr, ++counter);
            }
        }

        if (!placed)
        {
            ModPlayerPrefsHelper.SetModPriority(mod, ++counter);
        }

        SortAndAlertChange(__instance);

        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ModListView), nameof(ModListView.MoveToBottom))]
    public static bool BetterMoveToBottom(Mod mod, int index, VisualElement parent, ModListView __instance)
    {
        if (!MSettings.BetterModOrder) { return true; }

        var children = parent.Children();
        var count = children.Count();

        ModPlayerPrefsHelper.SetModPriority(mod, 1);
        for (int i = count - 1; i >= 0; i--)
        {
            if (index == i) { continue; } // Self

            var curr = __instance.GetModFromElement(parent.ElementAt(i));
            ModPlayerPrefsHelper.SetModPriority(curr, count - i + 1);
        }

        SortAndAlertChange(__instance);

        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ModPlayerPrefsHelper), nameof(ModPlayerPrefsHelper.IncreaseModPriority))]
    public static bool BetterIncreasePriority(Mod mod)
    {
        if (!MSettings.BetterModOrder) { return true; }

        ModPlayerPrefsHelper.SetModPriority(mod, GetOriginalIndex(ModPlayerPrefsHelper.GetModPriority(mod)) + 1);
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ModPlayerPrefsHelper), nameof(ModPlayerPrefsHelper.DecreaseModPriority))]
    public static bool BetterDecreasePriority(Mod mod)
    {
        if (!MSettings.BetterModOrder) { return true; }

        ModPlayerPrefsHelper.SetModPriority(mod, GetOriginalIndex(ModPlayerPrefsHelper.GetModPriority(mod)) - 1);
        return false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(ModUploaderBox), nameof(ModUploaderBox.Load))]
    public static void BetterUploadList(ModUploaderBox __instance)
    {
        var mods = __instance._mods;
        mods.Sort(new ModListComparer());

        var lst = __instance._modList;
        lst.itemsSource = null;
        lst.itemsSource = mods;
        lst.Rebuild();
    }

    [HarmonyPostfix, HarmonyPatch(typeof(ModUploaderBox), nameof(ModUploaderBox.BindItem))]
    public static void BetterUploadListName(ModUploaderBox __instance, VisualElement visualElement, int index)
    {
        var mod = __instance._mods[index];

        var enabled = ModPlayerPrefsHelper.IsModEnabled(mod);
        visualElement.Q<Label>("ModName").text = (enabled ? "" : "[DISABLED] ") + mod.DisplayName;
    }

    static void SortAndAlertChange(ModListView __instance)
    {
        __instance.SortList();

        // Use reflection to invoke the ListChanged event
        var listChangedEvent = typeof(ModListView).GetEvent(nameof(ModListView.ListChanged), AccessTools.all);
        if (listChangedEvent != null)
        {
            var eventDelegate = (MulticastDelegate)__instance.GetType()
                .GetField("ListChanged", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(__instance);
            if (eventDelegate != null)
            {
                foreach (var handler in eventDelegate.GetInvocationList())
                {
                    handler.Method.Invoke(handler.Target, [__instance, EventArgs.Empty]);
                }
            }
        }
    }

    static int GetSpacedOutPriority(int index) => (int)MathF.Round(BaseValue + index * MulValue);
    static int GetOriginalIndex(int spacedOutIndex) => (int)MathF.Round((spacedOutIndex - BaseValue) / MulValue);

}

class ModListComparer : IComparer<Mod>
{
    readonly Dictionary<Mod, bool> cache = [];

    public int Compare(Mod q1, Mod q2)
    {
        var enabled1 = cache.TryGetValue(q1, out var enabled) ? enabled : (cache[q1] = ModPlayerPrefsHelper.IsModEnabled(q1));
        var enabled2 = cache.TryGetValue(q2, out enabled) ? enabled : (cache[q2] = ModPlayerPrefsHelper.IsModEnabled(q2));

        return enabled1 == enabled2 ? q1.Manifest.Name.CompareTo(q2.Manifest.Name) : enabled2.CompareTo(enabled1);
    }
}
