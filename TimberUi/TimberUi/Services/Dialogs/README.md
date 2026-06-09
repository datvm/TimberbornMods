# Dialogs

## Purpose
Holds custom dialog `VisualElement`s that extend the base TimberUi `DialogBoxElement`. Currently a single type: `PromptDialogElement`, a modal text-input dialog (label + text field + OK/Cancel) used by `DialogService.PromptAsync` to ask the user for a string.

## Key types
- **`PromptDialogElement`** (`PromptDialogElement.cs`) — extends `DialogBoxElement`. Built in its constructor: a close button, a hidden-by-default game `Label` (`lblPrompt`), a `TextField` named `"PromptInput"`, and a row with OK/Cancel menu buttons wired to the inherited `OnUIConfirmed`/`OnUICancelled`. Fluent setters `SetPrompt(string)` (shows/hides the label based on emptiness) and `SetValue(string)` (seeds the text field). `ShowAsync(VisualElementInitializer, PanelStack)` starts the base show, focuses the text field, awaits the confirm result, and returns `txt.value` on confirm or `null` on cancel.

## How it fits together
`DialogService.PromptAsync` constructs a `PromptDialogElement(t)`, optionally calls `SetTitle`/`SetPrompt`/`SetValue`, then awaits `ShowAsync` — the returned `string?` flows straight back to the caller. The element relies entirely on the base `DialogBoxElement` for the confirm/cancel `Task<bool>` plumbing and panel lifecycle; this class only adds the text-input surface and the string result mapping.

## Dependencies & patterns
- Inherits the dialog lifecycle/`ShowAsync(...) : Task<bool>` machinery from `DialogBoxElement`; uses its `Content`, `AddCloseButton`, `OnUIConfirmed`, `OnUICancelled`.
- Builder/fluent extension helpers: `AddGameLabel`, `AddTextField`, `AddRow`, `AddMenuButton`, `SetDisplay`, `SetMarginBottom`.
- `ILoc t` only used to localize the "Core.OK"/"Core.Cancel" button labels.

## Usage notes
- `SetTitle` is inherited from `DialogBoxElement` and is called by `DialogService.PromptAsync` — it is not defined in this file.
- Call `SetPrompt(string)` with a non-empty string to display a prompt label above the text field; the label is hidden by default.
- The result contract is: confirmed ⇒ current text field value (may be `""`), cancelled ⇒ `null`. An empty-but-confirmed field returns `""` rather than `null`.
