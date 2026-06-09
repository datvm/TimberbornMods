# ModUpdate

## Purpose
Shows a one-time main-menu dialog when an enabled mod advertises a new version. A mod implements `IModUpdateNotifier` to declare its id, the version being announced, and a localization key for the changelog/message; `ModUpdateService` collects all such notifiers, filters to enabled mods and versions the player hasn't already dismissed, and presents per-mod update dialogs.

## Key types
- **`IModUpdateNotifier`** (`IModUpdateNotifier.cs`) — mod-implemented contract: `string ModId`, `string Version`, `string MessageLocKey`. No update *detection* logic — the mod author asserts "this version has an announcement"; the service handles dedup + display.
- **`ModUpdateService`** (`ModUpdateService.cs`) — `[BindSingleton(Contexts = MainMenu)]`, `IPostLoadableSingleton`. On `PostLoad`, `QueueMessages` builds a `Stack<ModNotifierPair>` of notifiers whose `ModId` matches an enabled mod's manifest and whose per-version `PlayerPrefs` key (`TimberUi.ModUpdate.{modId}.{version}`) is not yet set. `ShowNotificationsAsync` pops and shows each via `DialogBoxShower.ShowAsync` with Dismiss/Remind buttons; "Dismiss" (`result == true`) writes the PlayerPrefs key so that version won't show again.

## How it fits together
Pure registration + main-menu lifecycle. All bound `IModUpdateNotifier`s are gathered; cross-referenced against `ModRepository.EnabledMods` (so notifiers for disabled/absent mods are skipped); deduplicated against `PlayerPrefs`. The dialog text is composed from the mod's manifest `Name`, the notifier `Version`, and the localized `MessageLocKey`, wrapped in `LV.TimberUi.UpdateMessage`.

## Dependencies & patterns
- `IEnumerable<IModUpdateNotifier>` multi-injection; `ModRepository` for enabled-mod manifests; `ILoc` for text; `DialogBoxShower` for the modal.
- Per-version persistence via Unity `PlayerPrefs` (int 1 = dismissed). `ModNotifierPair` is a private `readonly record struct`.
- `Stack` is used so messages display in reverse registration/iteration order (LIFO).

## Usage notes
- The update dialog shows once per process launch per mod version. Returning to the main menu within the same session will not re-show the dialog.
- Choosing "Remind" stores nothing — the dialog will reappear on the next launch. Choosing "Dismiss" writes the `PlayerPrefs` key, permanently suppressing that version's announcement.
- The dedup key is `{ModId}.{Version}`. Bumping `Version` in the notifier causes the announcement to re-appear even if the mod content is otherwise unchanged, which allows authors to issue new announcements for new releases.
- Only enabled mods are checked: notifiers for mods not present in `ModRepository.EnabledMods` are skipped.
