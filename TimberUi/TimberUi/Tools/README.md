# Tools

## Purpose
Provides a single reusable Timberborn `ITool` — `SelectEntityTool` — that lets a mod ask the player to click on an entity in the world and returns the selected `EntityComponent` asynchronously. It wraps Timberborn's tool/cursor/input/highlight services so consuming mods don't have to implement entity-picking input handling themselves.

## Key types
- **`SelectEntityTool`** — the tool. Implements `ITool`, `IConstructionModeEnabler`, and `IInputProcessor`. Registered via `[BindSingleton(Contexts = BindAttributeContext.NonMenu)]` so it's available in-game (not in menus). Core API is `Task<EntityComponent?> SelectAsync(SelectEntityToolOptions)` — it switches the active tool to itself, awaits a click (or cancellation), and resolves the task with the picked entity or `null`.
- **`SelectEntityToolOptions`** — a `readonly record struct` configuring a selection session: optional `Source` `BaseComponent` (re-selected when the tool exits), `Cursor` name (defaults to `"PickObjectCursor"`), `HighlightColor` (defaults to `TimberUiUtils.SuccessColor`), and a `Filter` predicate `Func<SelectableObject, bool>` to restrict which objects are pickable.

## How it fits together
`SelectAsync` creates a `TaskCompletionSource<EntityComponent?>`, stores the options, and calls `toolService.SwitchTool(this)`. Timberborn then drives the tool lifecycle: `Enter()` registers this as an input processor and sets the cursor; `ProcessInput()` (called each frame) raycasts via `SelectableObjectRaycaster`, highlights a valid hit with `RollingHighlighter`, and on a left-click-not-over-UI calls `SetResult` (which completes the task and switches back to the default tool). `Exit()` cancels any pending task with `null`, removes the input processor, resets cursor/highlights, and re-selects `options.Source` via `EntitySelectionService`. The async/TCS bridge is the key pattern: callers `await tool.SelectAsync(...)` and get a clean result without subscribing to tool callbacks.

## Dependencies & patterns
- Injected Timberborn services: `ToolService`, `CursorService`, `InputService`, `RollingHighlighter`, `SelectableObjectRaycaster`, `EntitySelectionService`.
- Timberborn interfaces/types: `ITool`, `IConstructionModeEnabler`, `IInputProcessor`, `BaseComponent`, `EntityComponent`, `SelectableObject`.
- Uses `TimberUiUtils.SuccessColor` (from `../Helpers`) as the default highlight color.
- Pattern: `TaskCompletionSource` to convert an event/poll-driven tool into an awaitable; `record struct` options bag; defaulting via null-coalescing on each use.

## Notes
- Calling `SelectAsync` while a previous selection is still pending cancels the earlier one before starting the new session — the earlier `await` resolves to `null`. Both user cancellation and supersession by a new call yield `null`.
- The tool returns the `EntityComponent` of the hit object. If a selectable object has no `EntityComponent`, the result is `null`. The `Filter` predicate runs on `SelectableObject` before the `EntityComponent` lookup, so filtering is based on the selectable, not the entity component directly.
- `options` is reset in `Exit()`, so changing options mid-session is not supported.
