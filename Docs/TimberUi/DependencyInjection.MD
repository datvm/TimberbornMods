# Dependency Injection

TimberUi's DI layer turns what would be a wall of `Bind<T>().AsSingleton()` calls into a handful of attributes on your classes. You declare **one `AttributeConfigurator` subclass per game context**, decorate each class you want to register with a `[Bind…]` attribute (or the more specialised `[BindFragment]` / `[AddTemplateModule2]`), and TimberUi scans your assembly at configure time and wires everything for you. For the cases where attributes are not enough — overriding a game service, bulk-replacing bindings, or manually wiring a template-module decorator — a set of imperative fluent helpers on `Configurator` covers the gaps.

**Relevant source:** [`../../TimberUi/TimberUi/`](../../TimberUi/TimberUi/)

---

## Setting up the configurators

### Subclass one base per context

Timberborn's DI runs four separate `Configure()` passes, one per _game context_ (Bootstrapper, MainMenu, Game, MapEditor). TimberUi provides a matching abstract base for each:

| Base class | Context | Typical use |
|---|---|---|
| `BootstrapperAttributeConfigurator` | Bootstrapper | Services needed from game start before any scene |
| `MainMenuAttributeConfigurator` | Main menu | Services and UI only needed on the main-menu screen |
| `GameAttributeConfigurator` | In-game session | Services, fragments, template-module decorators |
| `MapEditorAttributeConfigurator` | Map editor | Same as Game but for the editor scene |

Subclass whichever contexts your mod needs and tag each with Timberborn's `[Context]` attribute. The `Configure()` method is already implemented on the base — it calls `this.BindAttributes(Context, GetAssembly())` — so an empty body is all you need:

```cs
[Context("Game")]
public class MyModGameConfig : GameAttributeConfigurator { }

[Context("MainMenu")]
public class MyModMenuConfig : MainMenuAttributeConfigurator { }
```

That is the entire setup. Any class in the same assembly that carries a matching `[Bind…]` attribute is picked up automatically.

### Scanning a different assembly

`GetAssembly()` defaults to `GetType().Assembly` — the assembly that contains your configurator subclass. If your `[Bind…]`-decorated types live in a separate assembly (a shared library, for example), override the property:

```cs
[Context("Game")]
public class MyModGameConfig : GameAttributeConfigurator
{
    public override Assembly GetAssembly() => typeof(MySharedLibraryAnchor).Assembly;
}
```

---

## The `BindAttributeContext` filter

Every `[Bind…]` attribute carries a `Contexts` flag that controls in which DI scope the type is registered. The enum members and their combinations are:

| Value | Numeric | Meaning |
|---|---|---|
| `MainMenu` | 1 | Main-menu scene only |
| `Game` | 2 | In-game session only |
| `MapEditor` | 4 | Map-editor scene only |
| `Bootstrapper` | 8 | Bootstrapper scope only |
| `All` | 7 | `MainMenu \| Game \| MapEditor` |
| `NonMenu` | 6 | `Game \| MapEditor` |

> `All` does **not** include `Bootstrapper`. A type decorated with `[Bind(Contexts = BindAttributeContext.All)]` is not registered in the bootstrapper scope.

The default value on all `[Bind…]` attributes is `Game` unless the attribute overrides it (e.g. `[BindMenuSingleton]` defaults to `MainMenu`).

---

## The attribute family

### `[Bind]` — the base attribute

[`[Bind]`](../../TimberUi/TimberUi/DIAttributes/BindAttribute.cs) is the general-purpose registration attribute. All the convenience attributes below are subclasses that preset one or more of its properties.

| Property | Type | Default | Meaning |
|---|---|---|---|
| `Contexts` | `BindAttributeContext` | `Game` | Which context(s) this registration applies to |
| `Scope` | `Scope?` | `null` (use configurator default) | `Singleton` or `Transient`; null inherits the configurator's default (singleton) |
| `As` | `Type?` | `null` (bind to self) | The service interface or base type to expose |
| `AlsoBindSelf` | `bool` | `false` | When `As` is set, also register the concrete type directly |
| `Exported` | `bool` | `false` | Mark the binding as exported (visible to parent containers) |
| `MultiBind` | `bool` | `false` | Register into a multibind collection of type `As` instead of a single binding |

`[Bind]` has `AllowMultiple = true`, so you can stack multiple attributes on the same class to register it in different ways — for example, as a game-context singleton _and_ a menu-context binding:

```cs
[Bind(Contexts = BindAttributeContext.Game,     Scope = Scope.Singleton, As = typeof(IMyService))]
[Bind(Contexts = BindAttributeContext.MainMenu, Scope = Scope.Transient)]
public class MyService : IMyService { /* ... */ }
```

### Convenience shorthand attributes

[`BindHelpers.cs`](../../TimberUi/TimberUi/DIAttributes/BindHelpers.cs) provides the shorthand attributes you will use day-to-day:

**`[BindSingleton]`** — binds the class to itself as a singleton in the `Game` context (or whatever `Contexts` you set):

```cs
[BindSingleton]
public class MyGameService { /* ... */ }
```

To expose an interface, set `As`:

```cs
[BindSingleton(As = typeof(IMyService))]
public class MyService : IMyService { /* ... */ }
```

**`[BindTransient]`** — same as `[BindSingleton]` but with `Scope.Transient`:

```cs
[BindTransient]
public class MyTransientHelper { /* ... */ }
```

**`[BindMenuSingleton]`** — shorthand for `[BindSingleton(Contexts = MainMenu)]`:

```cs
[BindMenuSingleton]
public class MyMenuService { /* ... */ }
```

**`[MultiBind(typeof(IService))]`** — registers the class into the multibind collection for `IService`. The `As` type is required (passed as a constructor argument):

```cs
[MultiBind(typeof(IMyListener))]
public class MyListener : IMyListener { /* ... */ }
```

To also make the concrete type directly resolvable alongside the multibind entry, set `AlsoBindSelf = true` on a plain `[Bind]`:

```cs
[Bind(As = typeof(IMyListener), MultiBind = true, AlsoBindSelf = true, Scope = Scope.Singleton)]
public class MyListener : IMyListener { /* ... */ }
```

### `[BindFragment]` — entity-panel fragments

[`[BindFragment]`](../../TimberUi/TimberUi/DIAttributes/BindHelpers.cs) marks a class as an entity-panel fragment. The class **must** implement `IEntityPanelFragment` (TimberUi validates this at configure time and throws if it does not). Fragment registration only runs in a `NonMenu` context (`Game` or `MapEditor`), so make sure the configurator that scans your assembly is one of `GameAttributeConfigurator` or `MapEditorAttributeConfigurator`.

If the class also implements `IEntityFragmentOrder`, TimberUi automatically calls `BindOrderedFragment` so the position ordering is wired too:

```cs
[BindFragment]
public class MyBuildingFragment : BaseEntityPanelFragment<MyComponent> { /* ... */ }
```

`[BindFragment]` does not carry a `Contexts` property — the active context drives it entirely.

### `[AddTemplateModule2]` — template-module decorators

[`[AddTemplateModule2]`](../../TimberUi/TimberUi/DIAttributes/AddTemplateModuleAttribute.cs) registers a class as a decorator on a Timberborn `TemplateModule` subject type. It is the current attribute for this purpose; the older `[AddTemplateModule]` is marked `[Obsolete]` and should be avoided in new code.

| Property | Type | Default | Meaning |
|---|---|---|---|
| `Subject` (ctor arg) | `Type` | required | The template type to decorate |
| `Contexts` | `BindAttributeContext` | `Game` | Which contexts this decorator applies to |
| `AlsoBindTransient` | `bool` | `true` | Automatically bind the decorator class as transient |

Template-module registration only runs in a `NonMenu` context. The `Contexts` property then further filters within that: a decorator with `Contexts = Game` is skipped when the active context is `MapEditor`, and vice versa.

```cs
[AddTemplateModule2(typeof(Workplace))]
public class MyWorkplaceDecorator { /* ... */ }
```

To apply the same decorator in both the game and map-editor scenes:

```cs
[AddTemplateModule2(typeof(Workplace), Contexts = BindAttributeContext.NonMenu)]
public class MyWorkplaceDecorator { /* ... */ }
```

---

## Adding extra bindings imperatively

When an attribute is not enough — conditional wiring, dynamic types, or overriding bindings the game registered — use the fluent `Configurator` extension methods. These live in [`Extensions/Configurators.cs`](../../TimberUi/TimberUi/Extensions/Configurators.cs) and [`Extensions/ConfiguratorHelpers.cs`](../../TimberUi/TimberUi/Extensions/ConfiguratorHelpers.cs) (namespace `Bindito.Core`), so they appear directly on `Configurator` inside any `Configure()` override.

All of these return `Configurator`, so they chain:

```cs
public override void Configure()
{
    base.Configure(); // run attribute scan first

    this.BindSingleton<MyExtraService>()
        .BindFragment<MyExtraFragment>();
}
```

### `BindSingleton` and `BindTransient`

Bind a single type or an interface→implementation pair at a fixed scope:

```cs
// Bind MyService to itself as a singleton
this.BindSingleton<MyService>();

// Bind IMyService → MyService as a singleton
this.BindSingleton<IMyService, MyService>();

// Bind MyHelper to itself as transient
this.BindTransient<MyHelper>();
```

### Multibinding

Add an implementation to a multibind collection and also register it as a singleton so it is directly resolvable:

```cs
// MultiBind<T> + Bind<TImpl>.AsSingleton() in one call
this.MultiBindAndBindSingleton<IMyListener, MyListener>();

// MultiBind only (no separate singleton binding)
this.MultiBindSingleton<IMyListener, MyListener>();
```

### Template-module decorators

Use `BindTemplateModule` with a lambda to add decorators in one fluent call. The lambda receives a `TemplateModuleHelper`, call `AddDecorator` on it, and return it — the helper's `Bind()` is called for you:

```cs
this.BindTemplateModule(h => h
    .AddDecorator<Workplace, MyWorkplaceDecorator>()
    .AddDecorator<Dwelling, MyDwellingDecorator>()
);
```

`AddDecorator<TSubject, TDecorator>` also binds the decorator class as transient by default (`addTransient = true`). Pass `false` to skip that if you are binding the decorator separately.

For multi-step setup, use the two-call form and call `Bind()` explicitly:

```cs
var helper = this.BindTemplateModule();
helper.AddDecorator<Workplace, MyWorkplaceDecorator>();
helper.Bind();
```

### Removing and rebinding game services

`RemoveBinding<T>()` removes an existing single binding from the container so you can replace it with your own:

```cs
this.RemoveBinding<ISomeGameService>();
this.BindSingleton<ISomeGameService, MyReplacementService>();
```

For replacing several services at once, `MassRebind` is more efficient — it batches the removes and re-adds in a single pass:

```cs
this.MassRebind(h => h
    .Replace<IFoo, MyFoo>()          // removes IFoo, binds IFoo → MyFoo as singleton
    .Replace<IBar, MyBar>()
    .Remove<IObsoleteService>()       // removes without replacement
);
```

`Replace<TRemove, TReplace>` also binds `TReplace` to itself by default (`alsoBindReplacement = true`). Pass `false` to suppress that.

> **Single bindings only.** `RemoveBinding` and `MassRebind` operate on single bindings only; they do not touch multibindings. To remove or replace a multibound game service use `RemoveMultibinding`/`RemoveMultibindings` or `MassRemoveMultibindings` instead.

### `IsBound` and `TryBind`

When writing a helper that may be called from multiple mods, guard registrations with `IsBound`/`TryBind` to avoid double-binding errors:

```cs
// Only register if nothing has claimed ISharedService yet
if (!this.IsBound<ISharedService>())
{
    this.BindSingleton<ISharedService, DefaultSharedService>();
}

// Equivalent one-liner: returns ISingleBindingBuilder<T> (null if already bound)
this.TryBind<ISharedService>()?.To<DefaultSharedService>().AsSingleton();
```

---

## Complete example

```cs
// -- MyMod/Configs/GameConfig.cs --

[Context("Game")]
public class MyModGameConfig : GameAttributeConfigurator
{
    public override void Configure()
    {
        base.Configure(); // attribute scan runs here

        // Extra wiring that attributes cannot express:
        this.BindTemplateModule(h => h
            .AddDecorator<Building, MyBuildingStats>()
        );
    }
}

// -- MyMod/Services/MyGameService.cs --

[BindSingleton]
public class MyGameService { /* ... */ }

// -- MyMod/Services/MyEventListener.cs --

[MultiBind(typeof(IGameEventListener))]
public class MyEventListener : IGameEventListener { /* ... */ }

// -- MyMod/Fragments/MyBuildingFragment.cs --

[BindFragment]
public class MyBuildingFragment : BaseEntityPanelFragment<Building> { /* ... */ }

// -- MyMod/Decorators/MyBuildingStats.cs --
// Registered imperatively above via BindTemplateModule because it needs
// a non-Game context variant in a sibling mod that uses MapEditor context.

public class MyBuildingStats { /* ... */ }
```

---

## Key types

| Type | Role |
|---|---|
| [`AttributeConfigurator`](../../TimberUi/TimberUi/AttributeConfigurator.cs) | Abstract base; `Configure()` calls `BindAttributes(Context, GetAssembly())`. Subclass via one of the four concrete bases. |
| [`GameAttributeConfigurator`](../../TimberUi/TimberUi/AttributeConfigurator.cs) | Ready-to-subclass base for the `Game` context |
| [`MainMenuAttributeConfigurator`](../../TimberUi/TimberUi/AttributeConfigurator.cs) | Ready-to-subclass base for the `MainMenu` context |
| [`MapEditorAttributeConfigurator`](../../TimberUi/TimberUi/AttributeConfigurator.cs) | Ready-to-subclass base for the `MapEditor` context |
| [`BootstrapperAttributeConfigurator`](../../TimberUi/TimberUi/AttributeConfigurator.cs) | Ready-to-subclass base for the `Bootstrapper` context |
| [`BindAttribute`](../../TimberUi/TimberUi/DIAttributes/BindAttribute.cs) | Base registration attribute; supports `Contexts`, `Scope`, `As`, `AlsoBindSelf`, `Exported`, `MultiBind` |
| [`BindAttributeContext`](../../TimberUi/TimberUi/DIAttributes/BindAttribute.cs) | `[Flags]` enum: `MainMenu`, `Game`, `MapEditor`, `Bootstrapper`, `All` (excludes Bootstrapper), `NonMenu` |
| [`BindSingletonAttribute`](../../TimberUi/TimberUi/DIAttributes/BindHelpers.cs) | `[Bind]` preset to `Scope.Singleton`, default context `Game` |
| [`BindTransientAttribute`](../../TimberUi/TimberUi/DIAttributes/BindHelpers.cs) | `[Bind]` preset to `Scope.Transient`, default context `Game` |
| [`BindMenuSingletonAttribute`](../../TimberUi/TimberUi/DIAttributes/BindHelpers.cs) | `[BindSingleton]` preset to `Contexts = MainMenu` |
| [`MultiBindAttribute`](../../TimberUi/TimberUi/DIAttributes/BindHelpers.cs) | `[Bind]` preset to `MultiBind = true`; requires the `As` type as a constructor argument |
| [`BindFragmentAttribute`](../../TimberUi/TimberUi/DIAttributes/BindHelpers.cs) | Marker attribute for `IEntityPanelFragment` types; only active in `NonMenu` contexts |
| [`AddTemplateModule2Attribute`](../../TimberUi/TimberUi/DIAttributes/AddTemplateModuleAttribute.cs) | Current template-module decorator attribute; supports `Subject`, `Contexts`, `AlsoBindTransient` |
| [`AttributeBinder`](../../TimberUi/TimberUi/DIAttributes/AttributeBinder.cs) | Static engine; called by `AttributeConfigurator.Configure()` to scan the assembly and apply all attributes |
| `UiBuilderExtensions` (in `Bindito.Core`) | Hosts `BindAttributes`, `BindSingleton`, `BindTransient`, `MultiBindSingleton`, `MultiBindAndBindSingleton`, `BindFragment`, `BindOrderedFragment`, `RemoveBinding`, `MassRemoveBindings`, `IsBound`, `TryBind`, `TryMultiBind` as extension methods on `Configurator` — source: [`Extensions/Configurators.cs`](../../TimberUi/TimberUi/Extensions/Configurators.cs) |
| [`TemplateModuleHelper`](../../TimberUi/TimberUi/Helpers/ConfiguratorHelpers.cs) | Fluent builder for `TemplateModule` decorator wiring; obtained via `configurator.BindTemplateModule()` |
| [`MassRebindingHelper`](../../TimberUi/TimberUi/Helpers/ConfiguratorHelpers.cs) | Batches `Replace` and `Remove` operations on existing bindings; obtained via `configurator.MassRebind()` |
| [`MConfigs`](../../TimberUi/TimberUi/MConfigs.cs) | TimberUi's own four configurators (`MBootstrapperConfig`, `MMenuConfig`, `MGameConfig`, `MMapConfig`) that register library-internal services — referenced here for orientation, not subclassed by consumer mods |
