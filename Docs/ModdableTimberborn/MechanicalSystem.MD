# Mechanical System

The Mechanical System subsystem makes a building's nominal power input and output runtime-moddable. Without it, those values are fixed at prefab-load time and cannot be changed by mods. Enabling the subsystem wraps every `MechanicalNode` in a decorator that runs a modifier pipeline, pushes recalculated values back into Timberborn's original component, and fires a change event for any downstream listeners (UI, logic, etc.).

## Enabling the subsystem

Call `UseMechanicalSystem()` on the shared registry inside your plugin's `Configure` method before the game context is built. The method is idempotent — calling it more than once is safe.

```cs
ModdableTimberbornRegistry.Instance
    .UseMechanicalSystem();
```

Source: [`ModdableMechanicalSystemConfig.cs`](../../ModdableTimberborn/MechanicalSystem/ModdableMechanicalSystemConfig.cs)

## Key types

| Type | Role |
|---|---|
| [`ModdableMechanicalNode`](../../ModdableTimberborn/MechanicalSystem/ModdableMechanicalNode.cs) | Decorator attached to every `MechanicalNode`. Owns the modifier collection, recalculates on dirty, and pushes updated values back into the wrapped node. |
| [`MechanicalNodeValues`](../../ModdableTimberborn/MechanicalSystem/MechanicalNodeValues.cs) | `readonly record struct` with two fields: `NominalInput` and `NominalOutput`. |
| [`ModdableMechanicalNodeValues`](../../ModdableTimberborn/MechanicalSystem/MechanicalNodeValues.cs) | `ModdableValue<MechanicalNodeValues>` wrapper passed through the modifier chain; exposes both the current (mutable) value and the original prefab value. |
| [`IModdableMechanicalNodeModifier`](../../ModdableTimberborn/MechanicalSystem/IModdableMechanicalNodeModifier.cs) | Interface your modifier component must implement. |

`MechanicalNodeModifierCollection` is a type alias for `ValueModifierCollection<IModdableMechanicalNodeModifier, ModdableMechanicalNodeValues, MechanicalNodeValues>` (defined in `zGlobalUsings.cs`).

## Writing a power modifier

A modifier is a `BaseComponent` (a Timberborn MonoBehaviour-like base) that implements `IModdableMechanicalNodeModifier`. The interface requires:

| Member | Purpose |
|---|---|
| `string Id { get; }` | Unique identifier for this modifier. |
| `int Priority { get; }` | Execution order — lower values run first. Use `ModifierPriority` enum values as a guide. |
| `bool Disabled { get; }` | When `true` the collection skips this modifier entirely. |
| `event Action? OnChanged` | Fire this whenever the modifier's state changes; the collection reacts by recalculating. |
| `bool Modify(ModdableMechanicalNodeValues value)` | Apply the modification in-place. Return `true` to short-circuit remaining modifiers, `false` to continue. |

`ModifierPriority` values: `Force = -1000`, `Additive = 0`, `Multiplicative = 1000`.

### Worked example

The demo project provides three modifier variants — additive, multiplicative, and force — built on a shared abstract base. The full source is at [`DemoMechanicalSystemModifier.cs`](../../ModdableTimberbornDemo/Features/MechanicalSystem/DemoMechanicalSystemModifier.cs).

```cs
// Shared base — handles Disabled/Amount state and fires OnChanged.
public abstract class BaseDemoMechanicalModifier : BaseComponent, IModdableMechanicalNodeModifier
{
    public abstract string Id { get; }
    public abstract int Priority { get; }
    protected bool ShouldShortCircuit { get; set; }

    public bool Disabled
    {
        get;
        set
        {
            if (value != field) { field = value; OnChanged?.Invoke(); }
        }
    } = true;

    public float Amount
    {
        get;
        set
        {
            if (value != field) { field = value; OnChanged?.Invoke(); }
        }
    }

    public event Action? OnChanged;

    public bool Modify(ModdableMechanicalNodeValues value)
    {
        var (curr, original) = value;

        // Only scale values that were non-zero on the original prefab.
        if (original.NominalInput != 0)
            value.Value = curr with { NominalInput = ModifyValue(curr.NominalInput) };

        if (original.NominalOutput != 0)
            value.Value = curr with { NominalOutput = ModifyValue(curr.NominalOutput) };

        return ShouldShortCircuit;
    }

    protected abstract int ModifyValue(int currValue);
}

// Additive modifier — priority 0; runs after Force (priority -1000).
public class DemoAdditiveMechanicalSystemModifier : BaseDemoMechanicalModifier
{
    public override string Id { get; } = nameof(DemoAdditiveMechanicalSystemModifier);
    public override int Priority { get; } = (int)ModifierPriority.Additive;

    protected override int ModifyValue(int currValue) => Mathf.RoundToInt(currValue + Amount);
}

// Multiplicative modifier — runs after additive (priority 1000).
public class DemoMultiplicativeMechanicalSystemModifier : BaseDemoMechanicalModifier
{
    public override string Id { get; } = nameof(DemoMultiplicativeMechanicalSystemModifier);
    public override int Priority { get; } = (int)ModifierPriority.Multiplicative;

    protected override int ModifyValue(int currValue) => Mathf.RoundToInt(currValue * Amount);
}

// Force modifier — short-circuits the chain (priority -1000).
public class DemoForceMechanicalSystemModifier : BaseDemoMechanicalModifier
{
    public override string Id { get; } = nameof(DemoForceMechanicalSystemModifier);
    public override int Priority { get; } = (int)ModifierPriority.Force;

    protected override int ModifyValue(int currValue) => Mathf.RoundToInt(Amount);

    public DemoForceMechanicalSystemModifier() { ShouldShortCircuit = true; }
}
```

Two runtime behaviours to keep in mind:

- **Modifiers start disabled.** The demo base class initialises `Disabled = true`, so a freshly registered modifier has no effect until your code sets `Disabled = false`. This matches the interface contract (`Disabled { get; }` — the collection skips disabled modifiers).
- **Chain resets before applying.** On every recalculation `ValueModifierCollection` resets `value.Value` to `value.OriginalValue` before iterating modifiers (`ValueModifierCollection.cs:23`). This means the modifier chain always starts from the prefab value, which is why additive and multiplicative ordering matters — a multiplicative modifier sees the additive-adjusted value, not the raw original.

### Registering modifiers

Add your modifier components as decorators on `MechanicalNode` via `BindTemplateModule`. Each instance is colocated on the same GameObject as the `MechanicalNode` it modifies. See [`MechanicalSystemConfig.cs`](../../ModdableTimberbornDemo/Features/MechanicalSystem/MechanicalSystemConfig.cs) in the demo:

```cs
public class MechanicalSystemConfig : IModdableTimberbornRegistryConfig
{
    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator
            .BindFragment<DemoMechanicalSystemFragment>()

            .BindTemplateModule(h => h
                .AddDecorator<MechanicalNode, DemoAdditiveMechanicalSystemModifier>()
                .AddDecorator<MechanicalNode, DemoMultiplicativeMechanicalSystemModifier>()
                .AddDecorator<MechanicalNode, DemoForceMechanicalSystemModifier>()
            );
    }
}
```

Pass this config to the registry:

```cs
ModdableTimberbornRegistry.Instance
    .UseMechanicalSystem()
    .AddConfigurator(new MechanicalSystemConfig());
```

## Reacting to value changes

`ModdableMechanicalNode` exposes an event you can subscribe to from UI fragments or other components:

```cs
var mechNode = entity.GetComponentFast<ModdableMechanicalNode>();
mechNode.OnMechanicalNodeValuesChanged += (_, e) =>
{
    // e.OldValue and e.NewValue are both MechanicalNodeValues
    Debug.Log($"Input: {e.OldValue.NominalInput} -> {e.NewValue.NominalInput}");
};
```

The current and original values are also available at any time via `mechNode.MechanicalNodeValues`. See [`DemoMechanicalSystemFragment.cs`](../../ModdableTimberbornDemo/Features/MechanicalSystem/DemoMechanicalSystemFragment.cs) for a full UI example that hides the panel when the node has zero input and output:

```cs
var (input, output) = mechNode.MechanicalNodeValues.OriginalValue;
if (input == 0 && output == 0)
{
    ClearFragment();
    return;
}
```

## Harmony patches

Enabling the subsystem also applies two Harmony patches (category `ModdableTimberborn.MechanicalSystem`). Both fix vanilla edge cases that surface when power values become dynamic:

- **`MechanicalGraphPatches`** — Adds a postfix to the `MechanicalGraph.Powered` getter. Vanilla returns `false` when `PowerDemand == 0`; the patch forces `true` in that case so networks with no consumers are not incorrectly treated as unpowered.
- **`MechanicalNodePatches`** — Adds a prefix to `MechanicalNode.CanPotentiallyBePowered` that returns `true` for any node whose nominal input is zero or below, bypassing the vanilla `NoPowerStatus` check.

Both patches are global — they affect all mechanical nodes and graphs, not only those with active modifiers.

Source: [`Patches/MechanicalGraphPatches.cs`](../../ModdableTimberborn/MechanicalSystem/Patches/MechanicalGraphPatches.cs), [`Patches/MechanicalNodePatches.cs`](../../ModdableTimberborn/MechanicalSystem/Patches/MechanicalNodePatches.cs)
