# DIAttributes

## Purpose
This folder is the engine of TimberUi's attribute-driven dependency injection. It defines the `[Bind]` attribute family that a mod author sticks on classes, and the `AttributeBinder` static class that scans an assembly and translates those attributes into Bindito container bindings. A mod author cares because this is what lets them write `[BindSingleton]` / `[MultiBind(typeof(IFoo))]` / `[BindFragment]` on a class and have it registered automatically per game state, instead of editing a `Configurator` by hand.

## Key types
- **`BindAttribute`** (`BindAttribute.cs`) — the base registration attribute (`AttributeTargets.Class`, `AllowMultiple`, not inherited). Properties: `Contexts` (default `Game`), `Scope?` (null = use configurator default), `As` (the service type to expose), `AlsoBindSelf`, `Exported`, `MultiBind`. Multiple `[Bind]`s on one class register it several ways.
- **`BindAttributeContext`** (`BindAttribute.cs`) — `[Flags]` enum (`MainMenu=1, Game=2, MapEditor=4, Bootstrapper=8`, `All=MainMenu|Game|MapEditor`, `NonMenu=Game|MapEditor`). The filter axis.
- **`BindSingletonAttribute` / `BindTransientAttribute`** (`BindHelpers.cs`) — `BindAttribute` subclasses that preset `Scope` to `Scope.Singleton` / `Scope.Transient`.
- **`BindMenuSingletonAttribute`** (`BindHelpers.cs`) — `BindSingleton` preset to `Contexts = MainMenu`.
- **`MultiBindAttribute`** (`BindHelpers.cs`) — ctor takes the `As` type and sets `MultiBind = true` (registers the class into a multibind collection of that service type).
- **`BindFragmentAttribute`** (`BindHelpers.cs`) — marker (no data) meaning "this `IEntityPanelFragment` should be registered as an entity-panel fragment."
- **`AddTemplateModuleAttribute`** (`AddTemplateModuleAttribute.cs`) — `[Obsolete]`, superseded by v2. Holds a `Subject` type and `AlsoBindTransient` (default true). Registers `this` class as a template-module decorator of `Subject`.
- **`AddTemplateModule2Attribute`** — the current version: adds `Contexts` (default `Game`) so decorators can be context-filtered. Holds `Subject`, `AlsoBindTransient`.
- **`AttributeBinder`** (`AttributeBinder.cs`) — static driver. `BindAttributes(assembly, configurator, defaultScope, context)` is the single entry point called by `Configurator.BindAttributes(...)`.

## How it fits together
`AttributeConfigurator.Configure()` → `Configurator.BindAttributes(context, assembly)` (extension in `Extensions/Configurators.cs`) → `AttributeBinder.BindAttributes(assembly, configurator, defaultScope, context)`.

Inside `AttributeBinder.BindAttributes`:
1. Computes `isNonMenu = (context & NonMenu) > 0` and `isGame = (context & Game) > 0`.
2. Loops over `assembly.GetTypes()`. For each type:
   - **`BindType`** reads every `[Bind]` on the type; skips any whose `Contexts` don't intersect the active `context`. For each surviving attribute it resolves `scope = attr.Scope ?? defaultScope` and binds:
     - **MultiBind path** — requires `As`; if `AlsoBindSelf`, binds the concrete type at scope and multibinds `As→type` with `toExisting:true` (the multibind reuses the separately self-bound singleton rather than creating a second instance), else multibinds directly. Honors `Exported`.
     - **Single path** — if `As` is null, `Bind(type).AsScope(scope)` (+ optional `AsExported`); else `BindExported(As, type, scope, AlsoBindSelf, Exported)`.
   - **`BindEntityFragment`** — only invoked when `isNonMenu`. If the type has `[BindFragment]`, asserts it implements `IEntityPanelFragment` (throws otherwise) and registers it via `BindOrderedFragment` (if it also implements `IEntityFragmentOrder`) or `BindFragment`.
   - **Template modules** — only when `isGame` for the v1 `[AddTemplateModule]`; the v2 `[AddTemplateModule2]` is processed whenever `isNonMenu` and filtered by its own `Contexts`. Each collects a `(Subject, type)` decorator pair and, if `AlsoBindTransient`, binds the decorator type as transient.
3. After the loop, if any decorators were collected, `BindTemplateModule` registers them all through one `configurator.BindTemplateModule(h => ... h.AddDecorator(sub, dec, false) ...)` call.

## Dependencies & patterns
- **Bindito** — `Configurator`, `Scope` (`Bindito.Core.Internal.Scope`), `IExportAssignee`, `MultiBind`, `Bind(...).AsScope(...)`, `BindTemplateModule`. The `BindExported` / `Bind(Type,...)` reflection-based helpers live in `Extensions/Configurators.cs`.
- **Timberborn** — `IEntityPanelFragment`, `IEntityFragmentOrder`, `TemplateModule` decorator system; `BindFragment`/`BindOrderedFragment` extensions (in `Extensions/`).
- **Pattern** — attribute discovery via reflection (`assembly.GetTypes()` + `GetCustomAttributes<T>()`); context flags as a bitmask filter.
- **Fail-loud** — invalid attribute combinations throw `InvalidOperationException` (e.g. `MultiBind`/`AlsoBindSelf` without `As`, `[BindFragment]` on a non-fragment). Misuse surfaces at configure time.

## Usage notes
- **Template modules and fragments require a `NonMenu` context.** A configurator running only in `MainMenu` will not process `[BindFragment]` or `[AddTemplateModule*]` registrations, as those paths are gated on `isNonMenu`. Use `[AddTemplateModule2]` (rather than the obsolete v1) to take advantage of context filtering.
- **`[BindFragment]` is gated by `isNonMenu`, not by the attribute's own `Contexts`.** Fragment registration applies in any non-menu context regardless of any `Contexts` value on a co-located `[Bind]` attribute.
- **`AllowMultiple = true` on `BindAttribute`** lets a type be registered several different ways; each attribute is filtered independently by `Contexts`, so one class can be e.g. a `Game` singleton and a separate `MainMenu` binding.
