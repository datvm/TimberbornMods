# OptionalMods

## Purpose

Provides a mechanism for a mod to ship DLLs that are only loaded at runtime when a specific *other* mod is also enabled. This avoids hard compile-time dependencies between mods: the optional DLL sits dormant unless its required companion mod is present, preventing load errors when that companion is absent.

## Key types

- **`OptionalModsLoader`** — Static utility that scans every enabled mod's directory for files matching the naming convention `*.dll.<targetModId>.optional`, loads them via `Assembly.Load`, and then walks their exported types to find and invoke `IModStarter` implementations.
- **`OptionalModStarter`** — The `IModStarter` entry point for this subsystem itself. Applies Harmony patches in the `ModdableTimberborn.OptionalMods` category on startup.
- **`OptionalModPatches`** — Harmony patch class (prefix on `ModAssetBundleLoader.Load`) that triggers `OptionalModsLoader.Load` at the right moment in Timberborn's mod-loading pipeline, but only when the call originates from `ModManagerScenePanel` to avoid double-invocation.

## How it fits together

`OptionalModStarter` is registered as a normal `IModStarter` entry point for ModdableTimberborn itself. When Timberborn initialises the mod, `OptionalModStarter.StartMod` runs Harmony, which patches `ModAssetBundleLoader.Load`.

When that patched method fires (from the Mod Manager scene), `OptionalModPatches.LoadOptionalMods` calls `OptionalModsLoader.Load(modRepository)`. The loader then:

1. Builds a set of all currently-enabled mod IDs.
2. For each enabled mod, finds any files named `*.dll.<modId>.optional` in the mod's directory tree.
3. Loads each file as an `Assembly` only if `<modId>` is in the enabled set.
4. For every loaded assembly, reflects over its types looking for concrete `IModStarter` implementations and instantiates + runs them via `starter.StartMod(ModEnvironment.Create(mod))`.

Consumers (other mods using ModdableTimberborn) do not interact with this subsystem directly; they simply name their optional DLL according to the convention and implement `IModStarter` inside it.

## Dependencies & patterns

- **Harmony** — `OptionalModStarter` applies a prefix patch on `ModAssetBundleLoader.Load`; the patch category is `ModdableTimberborn.OptionalMods`.
- **Timberborn internals** — `ModRepository`, `ModAssetBundleLoader`, `ModManagerScenePanel`, `Mod`, `IModStarter`, `ModEnvironment`, `IModEnvironment` (Timberborn's own mod API).
- **`TimberUiUtils.LogVerbose`** — Used for opt-in verbose tracing during load; not conditional-compiled, just behind a verbosity flag.
- No Bindito/DI registration — the loader is invoked purely through the Harmony patch, not via the DI container.

## Notes / gotchas

- **File naming is order-sensitive** — `GetModId` strips the `.optional` suffix, then reads everything after the first occurrence of `.dll.`. Name optional DLL files with a single `.dll.` segment before the mod ID to avoid ambiguity.
