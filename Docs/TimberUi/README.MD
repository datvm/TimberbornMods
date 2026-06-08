# TimberUi

**TimberUi** is a foundation library for Timberborn mods that need a user interface. It layers two things on top of the game's own systems:

1. An **attribute-driven dependency-injection layer** over Bindito — decorate a class with `[BindSingleton]`, `[MultiBind(...)]`, `[BindFragment]`, or `[AddTemplateModule2]` and a single per-context configurator wires it up automatically, instead of hand-writing `Bind<T>().AsSingleton()` for everything.
2. A large **fluent UI-building DSL** plus ready-made controls (dialogs, sliders, dropdowns, entity-panel fragments, alerts, bottom-bar and top-bar buttons) that target Timberborn's existing visual style, so your UI looks native with very little CSS.

Everything is **opt-in and incremental**: reference the assembly, register one configurator per game state, and use only the pieces you need. See [the Demo project](../../TimberUi/TimberUiDemo/) for runnable examples.

## Getting started

### 1. Depend on the library

Add TimberUi to your mod's `manifest.json` `RequiredMods` list and reference the assembly from your `.csproj`. TimberUi pulls in Bindito and Timberborn's UIElements/CoreUI, so its extension methods become available on the game's own types (`VisualElement`, `Configurator`, `BaseComponent`, …) just by referencing it.

### 2. Register a configurator per context

TimberUi's DI is attribute-driven. To switch it on for your mod, add one `Configurator` per game state (context) that scans your assembly for `[Bind…]` attributes. The simplest way is to subclass the matching `AttributeConfigurator` base and tag it with Timberborn's `[Context]` attribute:

```cs
[Context("MainMenu")]
public class MyMainMenuConfig : MainMenuAttributeConfigurator { }

[Context("Game")]
public class MyGameConfig : GameAttributeConfigurator { }
```

Each base's `Configure()` calls `this.BindAttributes(Context, GetAssembly())`, scanning **its own assembly** for attributes whose context matches. (Override `GetAssembly()` if your `[Bind…]` types live in a different assembly than the configurator.) The four bases are `BootstrapperAttributeConfigurator`, `MainMenuAttributeConfigurator`, `GameAttributeConfigurator`, and `MapEditorAttributeConfigurator`.

### 3. Declare your services and UI with attributes

```cs
[BindSingleton]                       // a Game-context singleton
public class MyService { /* ... */ }

[BindFragment]                        // an entity-panel inspector fragment
public class MyFragment : BaseEntityPanelFragment<MyComponent> { /* ... */ }
```

No imperative `Bind` calls required — the configurator from step 2 discovers and binds these. The full attribute set and the imperative helpers (for rebinding game services, multibinding, template modules) are covered in [Dependency Injection](./DependencyInjection.MD).

### 4. Build UI with the fluent DSL

Inside a fragment or dialog, assemble the visual tree with chainable `Add…`/`Set…` extensions:

```cs
var row = root.AddRow();
row.AddLabel("Name").SetMarginRight();
row.AddTextField(onValueChanged: OnNameChanged).SetFlexGrow();
row.AddButton("Save", onClick: Save, style: GameButtonStyle.Menu);
```

See [Building UI](./BuildingUi.MD) for the full builder catalog.

## Guides

- [Dependency Injection](./DependencyInjection.MD) — the `[Bind…]` attribute family, context filtering, and the imperative `Configurator` helpers for binding, multibinding, template modules, and rebinding game services.
- [Building UI](./BuildingUi.MD) — the fluent UiBuilder DSL: rows, labels, buttons, text/number fields, sliders, dropdowns, images, progress bars, rich text, and inline styling.
- [Entity Panel Fragments](./EntityPanelFragments.MD) — add inspector panels to buildings and characters with `BaseEntityPanelFragment<T>`, choose their position, and control their order.
- [Dialogs & Prompts](./DialogsAndPrompts.MD) — show alerts, confirmations, and text/number input dialogs with `DialogService` and the `DialogBoxElement` family.
- [Alerts & Bars](./AlertsAndBars.MD) — register alert-panel fragments, bottom-bar modules, and top-bar buttons.
- [Optional Services](./OptionalServices.MD) — opt-in services: camera shake, mod audio clips, native file dialogs, sprite operations, achievements, update notifications, and named icons.
- [Utilities](./Utilities.MD) — WAV loading, serializable collections, localization and persistence helpers, text-to-texture rendering, the entity-select tool, and USS/UXML export.
