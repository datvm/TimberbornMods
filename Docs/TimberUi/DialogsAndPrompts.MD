# Dialogs & Prompts

TimberUi provides two layers for showing modal dialogs. In almost every situation you want the **high-level `DialogService`** — inject it, call one method, await the result. When you need a fully custom dialog (custom layout, multiple fields, non-standard sizing) you drop down to **`DialogBoxElement`** and its subclasses and manage the panel yourself.

All dialogs push onto Timberborn's own `PanelStack`, so they participate in the game's modal stack, ESC handling, and the confirm/cancel lifecycle exactly like the game's own dialogs.

---

## `DialogService` — the recommended entry point

`DialogService` is a `[BindSingleton(Contexts = All)]` singleton. Inject it through primary-constructor DI in any service, fragment, or other singleton that needs to show a dialog.

```cs
[BindSingleton]
public class MyService(DialogService dialogs)
{
    // ...
}
```

### Showing an alert

`Alert` is fire-and-forget (no await required). `AlertAsync` returns a `Task` you can await if you need to know when the user dismissed it.

```cs
// Fire-and-forget — no await
dialogs.Alert("Operation complete.");

// Localized message key
dialogs.Alert("MyMod.OperationComplete", localized: true);

// Awaited — continues after the user clicks OK
await dialogs.AlertAsync("Something happened.");
```

Both overloads accept a `bool localized` parameter. When `true` the string is treated as a localization key and resolved via `ILoc.T(...)` before display; when `false` (the default) it is shown as-is.

### Asking for confirmation

`ConfirmAsync` shows a dialog with two buttons and returns `true` when the user confirms and `false` when they cancel.

```cs
bool confirmed = await dialogs.ConfirmAsync("Delete all saved data?");

if (confirmed)
{
    DeleteData();
}
```

The button labels default to the game's standard OK / Cancel strings. Pass `okText` / `cancelText` to override them with literal strings, or `localizedOkText` / `localizedCancelText` to supply localization keys:

```cs
// Literal button labels
bool ok = await dialogs.ConfirmAsync(
    "Overwrite the file?",
    okText: "Overwrite",
    cancelText: "Keep existing"
);

// Localized button labels
bool ok = await dialogs.ConfirmAsync(
    "MyMod.OverwritePrompt",
    localized: true,
    localizedOkText: "MyMod.Overwrite",
    localizedCancelText: "MyMod.KeepExisting"
);
```

When `localizedOkText`/`localizedCancelText` is provided it **overrides** the raw `okText`/`cancelText` (the raw value is discarded). If the localized key resolves to an empty string the dialog falls back to its default button text.

### Prompting for text input

`PromptAsync` shows a dialog with a text field and returns the entered string, or `null` if the user cancelled.

```cs
string? name = await dialogs.PromptAsync(
    title: "New preset",
    prompt: "Enter a name:",
    defaultValue: "My Preset"
);

if (name is not null)
{
    SavePreset(name);
}
```

All three parameters are optional. An empty-but-confirmed field returns `""` (not `null`), so check `string.IsNullOrEmpty` if a blank entry is not meaningful to your mod.

```cs
string? value = await dialogs.PromptAsync(prompt: "Value:");
if (string.IsNullOrEmpty(value))
    return;
```

---

## `DialogBoxElement` — building a custom modal

When you need a dialog that is more than a message and one or two buttons — multiple inputs, a custom layout, explicit sizing — build it directly from `DialogBoxElement`.

`DialogBoxElement` is a plain `VisualElement` subclass. Construct it, configure it, then call `Show` or `ShowAsync` to push it onto the panel stack.

### Minimal custom dialog

```cs
var dialog = new DialogBoxElement();
dialog.SetTitle("My Settings");
dialog.AddCloseButton();

// Add your own content into dialog.Content (a ScrollView)
dialog.Content.AddGameLabel("Configure things here.");

// Show and await — true = confirmed, false = cancelled/closed
bool confirmed = await dialog.ShowAsync(veInit, panelStack);
```

`VisualElementInitializer` and `PanelStack` are both game services — inject them wherever you create dialogs.

### Setting a title

```cs
dialog.SetTitle("Import Options");
```

`SetTitle` can be called before or after `Show`. The first call lazily adds the title chrome to the element.

### Adding a close button

```cs
dialog.AddCloseButton();                         // close = cancel
dialog.AddCloseButton(() => DoSomethingFirst()); // custom action instead
```

Without a custom action the close button wires to `OnUICancelled`. A custom action replaces that wiring; calling `AddCloseButton` a second time with a custom action throws, so set it once.

### Controlling dialog size

```cs
// Fixed pixel size (auto-scaled to screen DPI)
dialog.SetDialogSize(width: 600, height: 400);

// As a fraction of the screen (0.0–1.0)
dialog.SetDialogPercentSize(width: 0.4f, height: 0.3f);
```

Sizing can be called before `Show`; TimberUi defers the pixel calculation until after the dialog is attached to the panel (when DPI scale is available) and then applies it automatically.

### `Show` vs `ShowAsync`

```cs
// Callback-based — supply confirm/cancel actions directly
DialogBox box = dialog.Show(veInit, panelStack,
    confirm: () => Apply(),
    cancel:  () => Revert()
);

// Async — await a Task<bool>
bool ok = await dialog.ShowAsync(veInit, panelStack);
```

Both push the dialog onto `PanelStack` and return (or resolve) once the user confirms or cancels. The `confirm` / `cancel` callbacks are optional in the synchronous overload; when omitted they default to no-ops.

### Confirming and cancelling programmatically

```cs
dialog.OnUIConfirmed();   // confirm and close
dialog.OnUICancelled();   // cancel and close
dialog.Close();           // close without a confirm/cancel result
```

---

## `InputDialogBox` — typed value dialogs

`InputDialogBox` and its siblings extend `DialogBoxElement` with a prompt label, a typed input field, and OK/Cancel buttons. `ShowAsync` returns a value tuple `(bool Confirmed, TValueType? Value)`.

Three concrete types are available:

| Class | Input type | Value type |
|---|---|---|
| `InputDialogBox` | `NineSliceTextField` | `string` |
| `IntegerInputDialogBox` | `NineSliceIntegerField` | `int` |
| `FloatInputDialogBox` | `NineSliceFloatField` | `float` |

### Creating an instance

Use the static factories on `InputDialogBox` to keep construction consistent:

```cs
var stringDlg  = InputDialogBox.CreateString(t);
var intDlg     = InputDialogBox.CreateInteger(t);
var floatDlg   = InputDialogBox.CreateFloat(t);
```

Or construct the concrete subtype directly (useful when the CRTP fluent chain is important):

```cs
var dlg = new IntegerInputDialogBox(t);
```

`ILoc t` is the game's localization service — inject it into the surrounding singleton or fragment.

### Configuring and showing

The fluent setters (`SetPrompt`, `SetValue`, `AddCloseButton`) are chainable and return the concrete type (`InputDialogBox`, `IntegerInputDialogBox`, or `FloatInputDialogBox`), so the chain stays typed. `SetTitle` and `SetDialogSize` come from the base `DialogBoxElement` and return `DialogBoxElement`, which has no typed setters or tuple `ShowAsync` — call them on the variable as separate statements, not mid-chain.

```cs
var dlg = InputDialogBox.CreateInteger(t);
dlg.SetTitle("Worker count");
var (confirmed, value) = await dlg
    .SetPrompt("How many workers?")
    .SetValue(4)
    .AddCloseButton()
    .ShowAsync(veInit, panelStack);
if (confirmed)
{
    SetWorkers(value!.Value);
}
```

`AddCloseButton()` on `InputDialogBox` both adds the close button chrome and makes the Cancel button visible. If you omit it, the Cancel button is hidden and the user can only confirm.

When the dialog is cancelled `Confirmed` is `false` and `Value` is `null` for all value types — the field is `TValueType?`, so even `int` and `float` come back as `null`, not `0`. Always check `Confirmed` (or `value.HasValue`) before using `Value`.

### String input example

```cs
var dlg = InputDialogBox.CreateString(t);
dlg.SetTitle("Rename");
var (ok, name) = await dlg
    .SetPrompt("New name:")
    .SetValue(currentName)
    .AddCloseButton()
    .ShowAsync(veInit, panelStack);
if (ok && !string.IsNullOrEmpty(name))
{
    Rename(name);
}
```

### Float input example

```cs
var dlg = InputDialogBox.CreateFloat(t);
dlg.SetTitle("Set threshold");
var (ok, threshold) = await dlg
    .SetPrompt("Trigger at:")
    .SetValue(0.5f)
    .AddCloseButton()
    .ShowAsync(veInit, panelStack);
if (ok)
{
    _threshold = threshold!.Value;
}
```

---

## `DialogBoxShower.ShowAsync` — the low-level async extension

`TimberUiDialogExtensions.ShowAsync` (in `Extensions/Dialog.cs`) is a thin async wrapper around the game's own `DialogBoxShower` for cases where you want to build a dialog at the `DialogBoxShower.Builder` level rather than through `DialogService`.

```cs
// Inject DialogBoxShower directly
bool? result = await diagShower.ShowAsync(
    "Are you sure?",
    buttons: ["Yes", "No"]
);
```

The return type is `bool?`:

| Return value | Meaning |
|---|---|
| `true` | First button (confirm) clicked |
| `false` | Second button (cancel) clicked |
| `null` | Third button (info) clicked |

The `buttons` parameter is optional; omitting it shows the default OK button only. Supply up to three entries; any slot beyond the third is ignored. Passing `null` for a slot label uses the game's default text for that button.

```cs
// Single OK button (alert style)
await diagShower.ShowAsync("Done.");

// Two buttons
bool? ok = await diagShower.ShowAsync("Proceed?", ["Go", "Abort"]);

// Three buttons (confirm / cancel / info)
bool? choice = await diagShower.ShowAsync(
    "Choose an action",
    ["Apply", "Discard", "Later"]
);
```

By default the message is treated as a localization key (`messageLocalized: true`). Pass `messageLocalized: false` to display the string as-is:

```cs
await diagShower.ShowAsync(dynamicMessage, messageLocalized: false);
```

---

## Key types

| Type | Role | Source |
|---|---|---|
| [`DialogService`](../../TimberUi/TimberUi/Services/DialogService.cs) | `[BindSingleton(All)]` facade — `Alert`/`AlertAsync`, `ConfirmAsync`, `PromptAsync`. Recommended entry point. | `Services/DialogService.cs` |
| [`DialogBoxElement`](../../TimberUi/TimberUi/CommonUi/DialogBoxElement.cs) | Base `VisualElement` for custom modals. `SetTitle`, `AddCloseButton`, `SetDialogSize`/`SetDialogPercentSize`, `Show`/`ShowAsync`. | `CommonUi/DialogBoxElement.cs` |
| [`InputDialogBox`](../../TimberUi/TimberUi/CommonUi/InputDialogBox.cs) | Concrete string input dialog. Static factories `CreateString`/`CreateInteger`/`CreateFloat(ILoc)`. | `CommonUi/InputDialogBox.cs` |
| [`IntegerInputDialogBox`](../../TimberUi/TimberUi/CommonUi/InputDialogBox.cs) | `int` input dialog. | `CommonUi/InputDialogBox.cs` |
| [`FloatInputDialogBox`](../../TimberUi/TimberUi/CommonUi/InputDialogBox.cs) | `float` input dialog. | `CommonUi/InputDialogBox.cs` |
| [`InputDialogBox<TInput,TValueType>`](../../TimberUi/TimberUi/CommonUi/InputDialogBox.cs) | Base class for typed input dialogs. `ShowAsync` returns `(bool Confirmed, TValueType? Value)`. | `CommonUi/InputDialogBox.cs` |
| [`PromptDialogElement`](../../TimberUi/TimberUi/Services/Dialogs/PromptDialogElement.cs) | `DialogBoxElement` subclass used internally by `DialogService.PromptAsync`. Seldom used directly. | `Services/Dialogs/PromptDialogElement.cs` |
| [`TimberUiDialogExtensions.ShowAsync`](../../TimberUi/TimberUi/Extensions/Dialog.cs) | Extension method on `DialogBoxShower`; low-level async wrapper returning `bool?`. | `Extensions/Dialog.cs` |
