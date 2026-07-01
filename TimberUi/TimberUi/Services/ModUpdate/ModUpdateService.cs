namespace TimberUi.Services;

[BindSingleton(Contexts = BindAttributeContext.MainMenu)]
public class ModUpdateService(
    IEnumerable<IModUpdateNotifier2> notifiers,
    ModRepository modRepo,
    ILoc t,
    DialogService dialogService
) : IPostLoadableSingleton
{
    static bool showed = false;
    const string PrefId = "TimberUi.ModUpdate.{0}";

    public async void PostLoad()
    {
        if (showed) { return; }
        showed = true;

        var messages = QueueMessages();
        if (messages.Count == 0) { return; }

        await ShowNotificationsAsync(messages);
    }

    Stack<ModNotifierPair> QueueMessages()
    {
        var mods = modRepo.EnabledMods.ToDictionary(q => q.Manifest.Id, q => q.Manifest);

        Stack<ModNotifierPair> messages = [];

        foreach (var notifier in notifiers)
        {
            var id = notifier.ModId;
            if (!mods.TryGetValue(id, out var manifest)) { continue; }

            var storedVersion = GetStoredNumberVersion(id);
            if (storedVersion == -1) // New install, simply mark
            {
                SetStoredNumberVersion(id, notifier.VersionNumber);
                continue;
            }
            else if (storedVersion >= notifier.VersionNumber) // Already seen this version or newer
            {
                continue;
            }

            messages.Push(new(notifier, manifest));
        }

        return messages;
    }

    async Task ShowNotificationsAsync(Stack<ModNotifierPair> messages)
    {
        while (messages.Count > 0)
        {
            var (notifier, manifest) = messages.Pop();

            var result = await dialogService.ConfirmAsync(
                t.T("LV.TimberUi.UpdateMessage",
                    manifest.Name, notifier.Version,
                    t.T(notifier.MessageLocKey)),
                localizedOkText: "LV.TimberUi.UpdateDismiss",
                localizedCancelText: "LV.TimberUi.UpdateRemind");

            if (result == true)
            {
                SetStoredNumberVersion(notifier.ModId, notifier.VersionNumber);
            }
        }
    }

    public int GetStoredNumberVersion(string modId) => PlayerPrefs.GetInt(GetPrefId(modId), -1);
    public void SetStoredNumberVersion(string modId, int version) => PlayerPrefs.SetInt(GetPrefId(modId), version);

    static string GetPrefId(string modId) => string.Format(PrefId, modId);

    readonly record struct ModNotifierPair(IModUpdateNotifier2 Notifier, ModManifest Manifest);

}

