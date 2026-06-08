# Optional Services

Some TimberUi features are genuinely opt-in: they depend on game subsystems that not every mod needs, require a specific DI context to function, or must be shared safely across multiple mods that might all want the same service. Rather than registering these services unconditionally, TimberUi exposes them through a family of **`TryBinding*` helper methods** on `Configurator`. Call them from your configurator's `Configure()` override and get the services injected wherever you need them.

**Relevant source:** [`../../TimberUi/TimberUi/Extensions/OptionalServices.cs`](../../TimberUi/TimberUi/Extensions/OptionalServices.cs)

---

## The opt-in pattern

All `TryBinding*` methods are extension methods on `Configurator` (namespace `Bindito.Core`), so they appear directly inside any `Configure()` override. Every `TryBinding*` method is **idempotent**: if the service is already bound — by your own configurator or by another mod that ran first — the call is a no-op. You can therefore call these from multiple mods without coordination or double-bind errors.

`BindAchievement<T>` is **not** part of the `TryBinding*` family and is **not idempotent** — it uses a multibind and adds an entry on every call. Calling it twice for the same type will double-register that achievement.

The full set of opt-in helpers is:

| Method | Context requirement | What it registers |
|---|---|---|
| `TryBindingCameraShake(bool isMenuContext)` | Any | Camera-shake driver and settings toggle |
| `TryBindingModdableAudioClip()` | Intended for Bootstrapper (per XML docs) | WAV-file converter pipeline |
| `TryBindingAudioClipManagement()` | Any | Runtime add/replace/remove of clips |
| `TryBindingSystemFileDialogService()` | Any | Native OS open/save dialogs |
| `TryBindingSpriteOperations()` | Intended for MainMenu (per XML docs) | Sprite resize/deserialisation utilities |
| `BindAchievement<T>(bool bindSelf)` | Any | Achievement multibind registration (additive — not idempotent) |

`ModUpdateService` and `NamedIconProvider` are registered automatically by TimberUi's own configurators (`[BindSingleton(Contexts = MainMenu)]` and `[BindSingleton(Contexts = All)]` respectively); you enable their features by implementing an interface or injecting the provider, not by calling a `TryBinding*` method.

---

## Camera shake

### Enable

Call `TryBindingCameraShake` from any configurator that has access to the game session. The `isMenuContext` parameter tells the helper whether it is running in the main-menu scene; the shake *driver* (`CameraShakeService`) is not useful on the menu and is skipped there.

```cs
[Context("Game")]
public class MyModGameConfig : GameAttributeConfigurator
{
    public override void Configure()
    {
        base.Configure();
        this.TryBindingCameraShake(isMenuContext: false);
    }
}
```

If you also run a menu configurator and want the settings toggle available there, call it again with `isMenuContext: true` — it will register only the setting service and skip the shake driver.

```cs
[Context("MainMenu")]
public class MyModMenuConfig : MainMenuAttributeConfigurator
{
    public override void Configure()
    {
        base.Configure();
        this.TryBindingCameraShake(isMenuContext: true);
    }
}
```

### Use — `CameraShakeService`

Inject `CameraShakeService` into any game-scene singleton or component and call `Shake` when something impactful happens. Chain `MoveTo` before `Shake` to centre the camera on the object of interest first.

```cs
public class ExplosionHandler(CameraShakeService cameraShake)
{
    public void OnExplosion(SelectableObject source)
    {
        cameraShake
            .MoveTo(source)          // centres camera, then:
            .Shake(duration: 0.4f, strength: 1.5f);
    }
}
```

`Shake(float duration, float strength)` — `duration` is in seconds (uses `Time.unscaledDeltaTime`, so it runs even while paused); `strength` maps linearly to amplitude (`0.05 × strength`). Both `MoveTo` and `Shake` are silently suppressed when the player has disabled camera shake in settings.

`StopShaking()` is public if you need to cancel an in-progress shake early.

### Player setting — `CameraShakeSettingService`

`CameraShakeSettingService` is registered by the same `TryBindingCameraShake` call. It automatically injects a **"Disable camera shake"** toggle into the game's settings panel (placed after the UI scale row). You do not need to interact with it directly; it surfaces to the player automatically.

If your mod needs to read or respond to the player's preference programmatically, inject it directly:

```cs
public class MyConditionalService(CameraShakeSettingService shakeSetting)
{
    void OnSomething()
    {
        if (!shakeSetting.IsDisabled)
        {
            // shake is allowed
        }
    }
}
```

Subscribe to `OnCameraShakeDisabledChanged` to react at the moment the player flips the toggle.

**Source:** [`../../TimberUi/TimberUi/Services/CameraShake/CameraShakeService.cs`](../../TimberUi/TimberUi/Services/CameraShake/CameraShakeService.cs), [`../../TimberUi/TimberUi/Services/CameraShake/CameraShakeSettingService.cs`](../../TimberUi/TimberUi/Services/CameraShake/CameraShakeSettingService.cs)

---

## Mod audio clips

TimberUi's audio support comes in two complementary parts: a file-converter pipeline that loads `.wav` files from mod folders automatically, and a runtime service for registering or replacing clips in code.

### Enable — file-conversion pipeline (`TryBindingModdableAudioClip`)

Register `ModAudioClipConverter` during the **Bootstrapper** phase, before any mod assets are loaded. This is the context the file-converter pipeline is intended for (per its XML docs); calling it elsewhere is not a runtime error, but the pipeline assumes bootstrapper-phase lifecycle.

```cs
[Context("Bootstrapper")]
public class MyModBootstrapConfig : BootstrapperAttributeConfigurator
{
    public override void Configure()
    {
        base.Configure();
        this.TryBindingModdableAudioClip();
    }
}
```

Once registered, any `.wav` file found in your mod's file tree is decoded automatically and made available as a named `AudioClip` in the game's audio system. The clip's key in the audio service is the Unity `AudioClip.name` derived from the file.

### Enable — runtime clip management (`TryBindingAudioClipManagement`)

`AudioClipManagementService` does not depend on the bootstrapper pipeline — it can be registered in any context where you need to add, replace, or remove clips in code.

```cs
[Context("Game")]
public class MyModGameConfig : GameAttributeConfigurator
{
    public override void Configure()
    {
        base.Configure();
        this.TryBindingAudioClipManagement();
    }
}
```

### Use — `AudioClipManagementService`

```cs
public class MyAudioSetup(AudioClipManagementService audioClips)
{
    public void RegisterClips()
    {
        // Load a WAV from a known path and register it
        audioClips.AddOrReplace("C:/MyMod/sounds/alarm.wav");

        // Swap in a clip you already have a reference to
        audioClips.AddOrReplace("Alarm", myExistingClip);

        // Remove a clip that is no longer needed
        audioClips.Remove("Alarm");

        // Enumerate everything currently registered
        foreach (var (name, clip) in audioClips.AllAudioClips)
        {
            // ...
        }
    }
}
```

`AddOrReplace(string filePath, string? name = null)` loads the WAV file at `filePath` and registers it; the key used in the audio service is `AudioClip.name` as set by the loader. `AddOrReplace(string name, AudioClip clip)` lets you supply both the key and the clip directly. Only `.wav` files are supported; `AddOrReplace` calls the WAV loader directly and propagates whatever exception it throws on a corrupt or unreadable file. (`InvalidDataException` is thrown by the separate file-conversion pipeline in `ModAudioClipConverter`, not by `AddOrReplace` itself.)

**Source:** [`../../TimberUi/TimberUi/Services/ModAudioClip/AudioClipManagementService.cs`](../../TimberUi/TimberUi/Services/ModAudioClip/AudioClipManagementService.cs), [`../../TimberUi/TimberUi/Services/ModAudioClip/ModAudioClipConverter.cs`](../../TimberUi/TimberUi/Services/ModAudioClip/ModAudioClipConverter.cs)

---

## Native file dialogs

### Enable

```cs
[Context("Game")]
public class MyModGameConfig : GameAttributeConfigurator
{
    public override void Configure()
    {
        base.Configure();
        this.TryBindingSystemFileDialogService();
    }
}
```

The helper selects the correct implementation at runtime based on the operating system (Windows uses a P/Invoke against `comdlg32.dll`; Linux tries `zenity` then `kdialog`; macOS uses `osascript`). If you have already bound your own `ISystemFileDialogService` before this call, the call is a no-op and your implementation wins.

> **Platform caveat:** Only the Windows implementation is tested. The Linux and macOS implementations are AI-generated and are explicitly marked untested in source. Both swallow internal errors — a failure at the OS dialog level will look like a user cancel (returning `null`) rather than throwing. On an unsupported platform, `TryBindingSystemFileDialogService` binds nothing and logs an error, so any attempt to inject `ISystemFileDialogService` will fail to resolve there.

### Use — `ISystemFileDialogService`

```cs
public class MyExportHandler(ISystemFileDialogService fileDialog)
{
    public void ExportData()
    {
        // Prompt the user for a save location
        var path = fileDialog.ShowSaveFileDialog(filter: ".json");
        if (path is null) { return; } // user cancelled

        File.WriteAllText(path, BuildJson());
    }

    public void ImportData()
    {
        // Prompt the user to pick a file to open
        var path = fileDialog.ShowOpenFileDialog(filter: ".json;.txt");
        if (path is null) { return; } // user cancelled

        Process(File.ReadAllText(path));
    }
}
```

Both methods return `null` when the user cancels the dialog. The `filter` parameter is a semicolon-delimited list of extensions including the leading dot (e.g. `".json;.csv"`); pass `null` to show all files.

**Source:** [`../../TimberUi/TimberUi/Services/SystemFileDialog/ISystemFileDialogService.cs`](../../TimberUi/TimberUi/Services/SystemFileDialog/ISystemFileDialogService.cs)

---

## Sprite operations

### Enable

`TryBindingSpriteOperations` registers the `SpriteOperationsConfigurator`, which provides utilities for resizing sprites and deserialising `Spec` types that contain `Sprite` fields. It is intended to be called from a **MainMenu** configurator (per its XML docs), as the sprite pipeline it sets up assumes main-menu loading phase; this is a convention the pipeline relies on, not a runtime-enforced constraint.

```cs
[Context("MainMenu")]
public class MyModMenuConfig : MainMenuAttributeConfigurator
{
    public override void Configure()
    {
        base.Configure();
        this.TryBindingSpriteOperations();
    }
}
```

Calling it more than once (or from multiple mods) is safe; subsequent calls are no-ops.

**Source:** [`../../TimberUi/TimberUi/Extensions/OptionalServices.cs`](../../TimberUi/TimberUi/Extensions/OptionalServices.cs)

---

## Achievements

`BindAchievement<T>` registers an `Achievement` subclass into TimberUi's multibind collection for achievements.

### Enable and register

```cs
[Context("Game")]
public class MyModGameConfig : GameAttributeConfigurator
{
    public override void Configure()
    {
        base.Configure();

        // Register MyAchievement into the Achievement multibind only
        this.BindAchievement<MyAchievement>();

        // Also make MyAchievement directly injectable as itself
        this.BindAchievement<MyAchievement>(bindSelf: true);
    }
}
```

When `bindSelf` is `false` (the default) the type is multi-bound as an `Achievement` but is not directly injectable. Pass `bindSelf: true` to also register it as a singleton under its own concrete type, which lets other services inject `MyAchievement` directly.

**Source:** [`../../TimberUi/TimberUi/Extensions/OptionalServices.cs`](../../TimberUi/TimberUi/Extensions/OptionalServices.cs)

---

## Update notifications

`ModUpdateService` is registered automatically by TimberUi's main-menu configurator (`[BindSingleton(Contexts = MainMenu)]`) — no `TryBinding*` call needed. You enable it for your mod by implementing `IModUpdateNotifier` and registering it as a multibind entry.

### Implement `IModUpdateNotifier`

```cs
[MultiBind(typeof(IModUpdateNotifier))]
public class MyModUpdateNotifier : IModUpdateNotifier
{
    // The mod id as it appears in your mod's manifest.json
    public string ModId => "my.mod.id";

    // The version string for this announcement
    public string Version => "1.2.0";

    // A localization key whose value contains your changelog text
    public string MessageLocKey => "MyMod.Update.1_2_0";
}
```

At main-menu load, `ModUpdateService` cross-references every bound `IModUpdateNotifier` against the list of enabled mods, skips any version the player has already dismissed, and shows a modal dialog for each remaining announcement. The dialog offers **Dismiss** (remembers the decision via `PlayerPrefs` — the dialog won't appear again for that version) and **Remind** (shows again next launch).

The `Version` field versions the announcement, not the mod binary. Bumping it causes the dialog to re-appear even for players who are already on that mod version, so use it to distinguish distinct announcements rather than tracking binary releases.

**Source:** [`../../TimberUi/TimberUi/Services/ModUpdate/IModUpdateNotifier.cs`](../../TimberUi/TimberUi/Services/ModUpdate/IModUpdateNotifier.cs), [`../../TimberUi/TimberUi/Services/ModUpdate/ModUpdateService.cs`](../../TimberUi/TimberUi/Services/ModUpdate/ModUpdateService.cs)

---

## Named icons

`NamedIconProvider` is registered automatically by TimberUi's library configurators (`[BindSingleton(Contexts = All)]`) and is available in every game context. Inject it directly; no `TryBinding*` call is required.

### Use

```cs
public class MyFragment(NamedIconProvider icons)
{
    public void BuildUi(VisualElement parent)
    {
        // Use a built-in named sprite
        parent.AddImage(icons.Food);
        parent.AddImage(icons.Science);
        parent.AddImage(icons.QuestionMark);

        // Load and cache a custom sprite by name and asset path
        var myIcon = icons.GetOrLoad("MyCustomIcon", "sprites/mymod/my-icon");
        parent.AddImage(myIcon);

        // Shorthand helpers for common asset roots
        var topbarSprite = icons.GetOrLoadTopbar("MyTopbar", "my-topbar-icon");
        // resolves to: sprites/topbar/my-topbar-icon

        var gameSprite = icons.GetOrLoadGameIcon("MyGameIcon", "my-game-icon");
        // resolves to: ui/images/game/my-game-icon
    }
}
```

The built-in named properties are:

| Property | Asset path |
|---|---|
| `Food` | `sprites/topbar/Food` |
| `Logs` | `sprites/topbar/Logs` |
| `Materials` | `sprites/topbar/Materials` |
| `Science` | `sprites/topbar/Science` |
| `Water` | `sprites/topbar/Water` |
| `QuestionMark` | `ui/images/game/question-mark` |
| `Clock` | `ui/images/game/production-icon` |
| `Arrow` | `ui/images/game/Arrow` |
| `ScienceAlt` | `ui/images/game/science-icon` |

`GetOrLoad(name, path)` caches by both name and path, so the same asset loaded under different names is fetched from disk only once. Use the named properties or the `GetOrLoad*` helpers for first access; the indexer `provider["name"]` is a fast lookup but requires the sprite to have been loaded previously by a `GetOrLoad*` call.

**Source:** [`../../TimberUi/TimberUi/Services/NamedIconProvider.cs`](../../TimberUi/TimberUi/Services/NamedIconProvider.cs)

---

## Key types

| Type | Role | Source |
|---|---|---|
| `UiBuilderExtensions` (in `Bindito.Core`) | Hosts all `TryBinding*` helpers as extension methods on `Configurator` | [`../../TimberUi/TimberUi/Extensions/OptionalServices.cs`](../../TimberUi/TimberUi/Extensions/OptionalServices.cs) |
| `CameraShakeService` | Per-frame camera jitter driver; `MoveTo` + `Shake` + `StopShaking` | [`../../TimberUi/TimberUi/Services/CameraShake/CameraShakeService.cs`](../../TimberUi/TimberUi/Services/CameraShake/CameraShakeService.cs) |
| `CameraShakeSettingService` | Persisted on/off preference; injects settings-panel toggle automatically | [`../../TimberUi/TimberUi/Services/CameraShake/CameraShakeSettingService.cs`](../../TimberUi/TimberUi/Services/CameraShake/CameraShakeSettingService.cs) |
| `ModAudioClipConverter` | Bootstrapper-phase `.wav` → `AudioClip` converter; lifecycle-manages created clips | [`../../TimberUi/TimberUi/Services/ModAudioClip/ModAudioClipConverter.cs`](../../TimberUi/TimberUi/Services/ModAudioClip/ModAudioClipConverter.cs) |
| `AudioClipManagementService` | Runtime add/replace/remove of named clips in `AudioClipService` | [`../../TimberUi/TimberUi/Services/ModAudioClip/AudioClipManagementService.cs`](../../TimberUi/TimberUi/Services/ModAudioClip/AudioClipManagementService.cs) |
| `ISystemFileDialogService` | Cross-platform open/save file dialog contract; `TryBinding` selects the OS implementation | [`../../TimberUi/TimberUi/Services/SystemFileDialog/ISystemFileDialogService.cs`](../../TimberUi/TimberUi/Services/SystemFileDialog/ISystemFileDialogService.cs) |
| `IModUpdateNotifier` | Mod-implemented interface: `ModId`, `Version`, `MessageLocKey` | [`../../TimberUi/TimberUi/Services/ModUpdate/IModUpdateNotifier.cs`](../../TimberUi/TimberUi/Services/ModUpdate/IModUpdateNotifier.cs) |
| `ModUpdateService` | Collects notifiers, deduplicates by `PlayerPrefs`, shows main-menu dialogs | [`../../TimberUi/TimberUi/Services/ModUpdate/ModUpdateService.cs`](../../TimberUi/TimberUi/Services/ModUpdate/ModUpdateService.cs) |
| `NamedIconProvider` | Two-level sprite cache; named built-in properties + `GetOrLoad` / `GetOrLoadTopbar` / `GetOrLoadGameIcon` | [`../../TimberUi/TimberUi/Services/NamedIconProvider.cs`](../../TimberUi/TimberUi/Services/NamedIconProvider.cs) |
