# TimberUi

## Purpose
TimberUi is a Timberborn modding library that layers an **attribute-driven dependency-injection and UI-building toolkit** on top of Bindito (Timberborn's DI container) and Unity UIElements. Instead of hand-writing a `Configurator` that calls `Bind<T>().AsSingleton()` for every service, a mod author decorates classes with `[Bind]`, `[BindSingleton]`, `[MultiBind]`, `[BindFragment]`, `[AddTemplateModule2]`, etc., and a single per-context `AttributeConfigurator` scans the assembly and wires everything automatically. On top of the DI layer sits a large set of UI builder extensions and ready-made UIElements controls (dialogs, sliders, dropdowns, entity-panel fragments, bottom-bar buttons, alerts) plus CSS-class and style-enum constants that mirror Timberborn's own USS.

A mod author cares because TimberUi removes most of the Bindito boilerplate, exposes the same registration surface Timberborn uses internally (entity-panel fragments, alerts, bottom-bar modules, template-module decorators), and gives a fluent UI-construction API that targets the game's existing visual style.

## Key types
- **`AttributeConfigurator`** (`AttributeConfigurator.cs`) — abstract `Bindito.Configurator` base. Its `Configure()` calls `this.BindAttributes(Context, GetAssembly())`, scanning the subclass's own assembly. `Context` (abstract) selects which `BindAttributeContext` is active; `GetAssembly()` defaults to `GetType().Assembly` (override to scan a different assembly).
- **`MainMenuAttributeConfigurator` / `GameAttributeConfigurator` / `MapEditorAttributeConfigurator` / `BootstrapperAttributeConfigurator`** — the four concrete bases, one per `BindAttributeContext` value. A mod subclasses the one(s) it needs.
- **`MBootstrapperConfig` / `MMenuConfig` / `MGameConfig` / `MMapConfig`** (`MConfigs.cs`) — TimberUi's own four ready-to-register configurators (trivial subclasses of the four bases). Each carries Timberborn's `[Context(nameof(...))]` attribute so the game's mod loader registers it into the matching DI scope. These are what actually pull TimberUi's library services into each game state.
- **`BindAttributeContext`** (`DIAttributes/BindAttribute.cs`) — `[Flags]` enum: `MainMenu=1, Game=2, MapEditor=4, Bootstrapper=8`, plus combinations `All = MainMenu|Game|MapEditor` and `NonMenu = Game|MapEditor`. The axis along which every `[Bind]` / template-module attribute is filtered.
- **`UiCssClasses`** (`UiCssClasses.cs`) — `static partial` bag of string constants and `ImmutableArray<string>` bundles naming Timberborn's USS classes (button variants, fragment backgrounds, label/text styles, slider/toggle/scrollview classes, color names). Used by the UI builder extensions so generated controls match the game skin.
- **`UiEnums`** (`UiEnums.cs`, namespace `UiBuilder`) — style/size/color enums (`GameLabelStyle`, `GameLabelSize`, `GameLabelColor`, `GameButtonStyle`, `GameButtonSize`, `ToggleStyle`) consumed by the builder extensions to pick a CSS-class set.

## How it fits together
1. Timberborn's mod loader instantiates the `[Context(...)]`-decorated configurators (`MGameConfig`, etc.) into the corresponding DI scope.
2. Each configurator's `Configure()` runs `BindAttributes(Context, assembly)`, which delegates to `AttributeBinder.BindAttributes` (see `DIAttributes/`).
3. `AttributeBinder` enumerates every type in the assembly and, for each, reads its `[Bind]`-family attributes, filtering by whether the attribute's `Contexts` flags intersect the active context. Matching types are bound into the container (singleton/transient/multibind/exported), entity-panel fragments are registered, and `[AddTemplateModule2]` decorators are collected into a single template-module binding.
4. At runtime the consumer resolves those services (or the game resolves the multibound fragments/alerts/bottom-bar modules) and uses the UI builder extensions + `UiCssClasses`/`UiEnums` to construct visuals.

So the consumer's only required call is "subclass one `AttributeConfigurator` per context and register it" — everything else is discovered from attributes.

## Dependencies & patterns
- **Bindito.Core** — `Configurator`, `Scope`, `IProvider<T>`, `MultiBind`, `BindExported`. The whole DI surface is Bindito.
- **Timberborn** — `[Context]` attribute and mod-loading lifecycle; `EntityPanelModule`, `AlertPanelModule`, `BottomBarModule`, `IEntityPanelFragment`, `TemplateModule`/decorators; UIElements (`VisualElement`, USS).
- **Unity UIElements / UnityEditor** — global usings pull in `UnityEditor.StyleSheets` and `UnityEditor.UIElements.Debugger` (used by `UnityUiElements/` USS-export tooling). Note these are editor namespaces.
- **Pattern: attribute-driven registration** — declarative `[Bind]` attributes replace imperative configurator code; context flags give per-game-state activation. `zGlobalUsings.cs` centralizes namespace imports (note aliases `NineSliceFloatField` and `TProgressBar = Timberborn.CoreUI.ProgressBar`).

## Subfolder map
- **`DIAttributes/`** — the `[Bind]`/`[AddTemplateModule]` attributes and `AttributeBinder` that applies them (the DI heart).
- **`CommonProviders/`** — reusable Bindito `IProvider<...>` base classes for fragments, alerts, bottom-bar modules, dropdown items.
- **`CommonUi/`** — concrete UIElements controls (dialog boxes, sliders, dropdown rows, progress bars, toggle groups, entity-panel fragment scaffolding, alert elements).
- **`Extensions/`** — the bulk of the fluent API: `Configurator` binding helpers (`Configurators.cs`, `ConfiguratorHelpers.cs`) and dozens of UI builder extensions (buttons, texts, sliders, dropdowns, dialogs, images, text fields, progress bars).
- **`Helpers/`** — utilities: list extensions, serializable float/int wrappers, `TimberUiUtils`, WAV loading, configurator helpers.
- **`Services/`** — runtime services: dialogs, camera shake, mod audio clips, mod update checks, entity-panel ordering, named icons, system file dialog, min/max slider init, text textures.
- **`Tools/`** — `SelectEntityTool` (a Timberborn `Tool`).
- **`UnityUiElements/`** — editor-side USS/UXML export tooling (`StyleSheetToUss`, `UxmlExporter`).
- **`Localizations/`** — `enUS.csv` localization keys shipped by the library.

## Notes
- **`BindAttributeContext.All` = `MainMenu|Game|MapEditor` (not `Bootstrapper`).** A `[Bind(Contexts = All)]` type is not registered in the bootstrapper scope.
- The four `MConfigs` rely on Timberborn's `[Context(string)]` attribute matching the loader's expected context names; the `nameof(BindAttributeContext.X)` strings must equal what Timberborn expects, so TimberUi's enum names are coupled to Timberborn's context naming.
- `GetAssembly()` defaults to the configurator subclass's assembly — a mod that puts its configurator in a different assembly than its `[Bind]` types must override `GetAssembly()`, or those types will not be scanned.
- `EntityPanelFragmentPosition.RightHeader` is `[Obsolete]`; prefer the current position values (see `CommonProviders`).
