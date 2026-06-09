# EntityPanelOrder

## Purpose
Lets mods control the vertical ordering of their fragments within the in-game entity selection panel relative to vanilla fragments. A mod registers an `IEntityFragmentOrder` (binding a `VisualElement` to an integer `Order`); `EntityFragmentOrderService` runs once on load and re-inserts those fragments so negative-ordered ones float to the top and positive-ordered ones sink to the bottom.

## Key types
- **`IEntityFragmentOrder`** (`IEntityFragmentOrder.cs`) — contract a mod implements per orderable fragment. `int Order { get; }` (negative ⇒ guaranteed above vanilla fragments, positive ⇒ guaranteed below, 0 ⇒ neutral / no repositioning) and `VisualElement Fragment { get; }` (the element to move).
- **`EntityFragmentOrderService`** (`EntityFragmentOrderService.cs`) — `[BindSingleton(Contexts = NonMenu)]`, `ILoadableSingleton`. Collects all `IEnumerable<IEntityFragmentOrder>`, sorts by `Order`, and on `Load` walks them: negatives are inserted from index 0 downward (chained via `InsertSelfAfter`), positives are appended after the last existing child and chained after each other. Also injects `IEntityPanel` purely as a DI ordering dependency (unread — `CS9113` suppressed) to guarantee the panel is constructed first.

## How it fits together
This is a registration-driven service: any number of `IEntityFragmentOrder` implementations bound into the container are gathered and applied in one pass. It mutates the live entity panel's `VisualElement` tree (`panel.parent.Insert` / `InsertSelfAfter`), so it must run after the panel and all fragments exist — hence the unused `IEntityPanel` constructor dependency forcing DI order, and the `NonMenu` context (no entity panel in the main menu).

## Dependencies & patterns
- Multi-injection of `IEnumerable<IEntityFragmentOrder>` (the standard "collect all implementers" pattern).
- `InsertSelfAfter` extension and Unity `VisualElement.parent.Insert` / `.Children().Last()` for DOM reordering.
- Deliberate unread-parameter DI-ordering trick (documented inline with `#pragma warning disable CS9113`).

## Usage notes
- `Order` values: negative integers place a fragment above vanilla content, positive integers place it below, and `0` leaves the fragment in its default position.
- When multiple fragments share the same `Order` value, their relative ordering follows the sort-stable order they appear in after `OrderBy` — assign distinct values if precise relative ordering between mod fragments matters.
- All registered `Fragment` elements must already be attached to the panel's `VisualElement` tree when `Load` runs. The ordering pass is applied once at load time.
