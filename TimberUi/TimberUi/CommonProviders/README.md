# CommonProviders

## Purpose
Reusable Bindito `IProvider<...>` base classes that wrap Timberborn's various UI "module" builders. Timberborn registers entity-panel fragments, alerts, and bottom-bar elements indirectly: you multibind a provider that yields an `EntityPanelModule` / `AlertPanelModule` / `BottomBarModule`, and the game's UI assembles those modules. These classes save the mod author from writing the builder boilerplate by hand — typically they're referenced from the `Configurator.Bind*` extension helpers (e.g. `BindFragment<T>` multibinds `EntityPanelFragmentProvider<T>`, `BindAlertFragment<T>` multibinds `AlertFragmentProvider<T>`) rather than instantiated directly.

## Key types
- **`EntityPanelFragmentProvider<T>`** (`EntityPanelFragmentProvider.cs`) — generic convenience: wraps a single `T : IEntityPanelFragment` into the non-generic provider at the default position (`Top`).
- **`EntityPanelFragmentProvider`** (non-generic) — `IProvider<EntityPanelModule>`. Holds an `IEnumerable<EntityPanelRegistration>`. `Get()` builds an `EntityPanelModule.Builder`, then for each registration switches on `Position` and calls the matching `Add*Fragment` builder method (Top/Bottom/Diagnostic/Footer/LeftHeader/Middle/MiddleHeader/Side; LeftHeader passes `Order`). Has convenience ctors taking `IEnumerable<IEntityPanelFragment>` (+ optional `EntityPanelFragmentPosition`).
- **`EntityPanelRegistration`** — `readonly record struct (IEntityPanelFragment Fragment, EntityPanelFragmentPosition Position, int Order = 0)`.
- **`EntityPanelFragmentPosition`** — enum of fragment slots in the entity panel. Includes an `[Obsolete] RightHeader`.
- **`AlertFragmentProvider<T>`** (`AlertFragmentProvider.cs`) — `IProvider<AlertPanelModule>` where `T : IAlertFragmentWithOrder`. `Get()` builds an `AlertPanelModule` adding the single fragment with its `Order`.
- **`BottomBarModuleProvider<T>`** (`BottomBarModuleProvider.cs`) — `IProvider<BottomBarModule>` where `T : IBottomBarElementsProvider`. `Get()` adds the element to the bottom bar's **right** section and builds.
- **`SimpleDropdownItemProvider`** (`SimpleDropdownItemProvider.cs`) — `IDropdownProvider` backed by an in-memory `IReadOnlyList<string>` with a mutable selected `value` (defaults to `defaultValue ?? items.First()`). Implements `GetValue`/`SetValue`.

## How it fits together
A configurator multibinds one of these providers against the game's module collection (e.g. `MultiBind<EntityPanelModule>().ToProvider<EntityPanelFragmentProvider<T>>()`). When Timberborn constructs the relevant panel/bar, it resolves all providers, calls `Get()` on each to obtain a built module, and merges them. The provider receives the actual fragment/element instance via constructor injection (Bindito resolves `T`), so the fragment itself is a normally-bound service. `SimpleDropdownItemProvider` is different: it's the value-backing model handed to a dropdown control, not a module provider.

## Dependencies & patterns
- **Bindito** — `IProvider<T>` is the provider contract; these are designed to be multibound via `.ToProvider<...>()`.
- **Timberborn** — `EntityPanelModule(.Builder)`, `AlertPanelModule(.Builder)`, `BottomBarModule(.Builder)`, `IEntityPanelFragment`, `IBottomBarElementsProvider`, `IDropdownProvider`; `IAlertFragmentWithOrder` is TimberUi's own interface (see `CommonUi/IAlertFragmentWithOrder.cs`).
- **Pattern** — thin builder-wrapping providers + primary-constructor injection. The generic→non-generic delegation in `EntityPanelFragmentProvider` keeps a single `Get()` implementation.

## Usage notes
- **Fixed section for `BottomBarModuleProvider`:** this provider always targets the **right** section (`AddRightSectionElement`). For left-section placement, implement `IProvider<BottomBarModule>` directly.
- **Fixed position for the generic `EntityPanelFragmentProvider<T>`:** the single-fragment generic variant always uses `Top`. To place a fragment at a different position, use the non-generic ctor with an explicit `EntityPanelFragmentPosition`.
- **`[Obsolete] RightHeader`:** `EntityPanelFragmentPosition.RightHeader` is deprecated; use one of the current position values instead.
- **`Order` on `EntityPanelRegistration`:** the `Order` field is used by the `LeftHeader` position. For other positions, ordering is determined by the multibind registration sequence.
- **`SimpleDropdownItemProvider` initial value:** when no `defaultValue` is supplied, the selected value is initialized to the first element of `items`. Ensure `items` is non-empty when using this constructor overload.
