namespace TimberUi.Services;

public class ModUpdateService(
    IEnumerable<IModUpdateNotifier> notifiers,
    ModRepository modRepo,
    ILoc t,
    DialogBoxShower diag
) : IPostLoadableSingleton
{
    static bool showed = false;
    const string PrefId = "TimberUi.ModUpdate.{0}.{1}";

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

            var prefId = GetPrefId(id, notifier.Version);
            if (PlayerPrefs.HasKey(prefId)) { continue; }

            messages.Push(new(notifier, manifest));
        }

        return messages;
    }

    async Task ShowNotificationsAsync(Stack<ModNotifierPair> messages)
    {
        while (messages.Count > 0)
        {
            var (notifier, manifest) = messages.Pop();

            var result = await diag.ShowAsync(
                t.T("LV.TimberUi.UpdateMessage",
                    manifest.Name, notifier.Version,
                    t.T(notifier.MessageLocKey)),
                [t.T("LV.TimberUi.UpdateDismiss"), t.T("LV.TimberUi.UpdateRemind")],
                false);

            if (result == true)
            {
                var prefId = GetPrefId(notifier.ModId, notifier.Version);
                PlayerPrefs.SetInt(prefId, 1);
            }
        }
    }

    static string GetPrefId(string modId, string version) => string.Format(PrefId, modId, version);

    readonly record struct ModNotifierPair(IModUpdateNotifier Notifier, ModManifest Manifest);

}

