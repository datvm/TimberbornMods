using static UnityEngine.PlayerPrefs;

namespace SyncableSettings.Patches;

[HarmonyPatch(typeof(PlayerPrefs))]
public static class PlayerPrefsPatches
{
    static readonly PlayerPrefDb db = PlayerPrefDb.Instance;

    [HarmonyPrefix, HarmonyPatch(nameof(DeleteAll))]
    public static void PrefixDeleteAll() => db.DeleteAll();

    [HarmonyPrefix, HarmonyPatch(nameof(DeleteKey))]
    public static void PrefixDeleteKey(string key) => db.DeleteKey(key);

    [HarmonyPrefix, HarmonyPatch(nameof(GetFloat), [typeof(string), typeof(float)])]
    public static bool PrefixGetFloat(string key, ref float __result) => !db.TryGetFloat(key, ref __result);

    [HarmonyPrefix, HarmonyPatch(nameof(GetInt), [typeof(string), typeof(int)])]
    public static bool PrefixGetInt(string key, ref int __result) => !db.TryGetInt(key, ref __result);

    [HarmonyPrefix, HarmonyPatch(nameof(GetString), [typeof(string), typeof(string)])]
    public static bool PrefixGetString(string key, ref string __result) => !db.TryGetString(key, ref __result);

    [HarmonyPrefix, HarmonyPatch(nameof(HasKey))]
    public static bool PrefixHasKey(string key, ref bool __result) => !db.TryHasKey(key, ref __result);

    [HarmonyPrefix, HarmonyPatch(nameof(Save))]
    public static void PrefixSave() => db.Save();

    [HarmonyPrefix, HarmonyPatch(nameof(SetFloat))]
    public static void PrefixSetFloat(string key, float value) => db.SetFloat(key, value);

    [HarmonyPrefix, HarmonyPatch(nameof(SetInt))]
    public static void PrefixSetInt(string key, int value) => db.SetInt(key, value);

    [HarmonyPrefix, HarmonyPatch(nameof(SetString))]
    public static void PrefixSetString(string key, string value) => db.SetString(key, value);
}
