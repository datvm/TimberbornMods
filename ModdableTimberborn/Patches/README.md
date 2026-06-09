# Patches

## Purpose

Contains Harmony postfix patches that hook into Timberborn's internal infrastructure at points where the game's own code runs but mod code has no direct access. Currently the sole concern is intercepting DI container creation so ModdableTimberborn can hold a live reference to the Bindito `IContainer` for later imperative service resolution.

## Key types

- **`ContainerRetrieverPatches`** — Static Harmony patch class. Postfixes `ContainerCreator.CreateContainer` to capture the freshly-built `IContainer` and hand it to `ContainerRetriever`.

## How it fits together

When Timberborn bootstraps a scene it calls `ContainerCreator.CreateContainer`, which builds and returns a Bindito `IContainer`. The `[HarmonyPostfix]` on that method fires immediately after, receiving `__result` (the new container), and passes it straight to `new ContainerRetriever(__result)` in `ModdableTimberborn.Services`. `ContainerRetriever` stores the container behind a `WeakReference` so it doesn't prevent GC if the scene unloads, and exposes static `GetInstance<T>()` / `GetInstances<T>()` helpers that the rest of ModdableTimberborn uses to resolve services imperatively (i.e., outside normal constructor injection).

The Patches folder is therefore the sole bridge between Timberborn's container lifecycle and ModdableTimberborn's service-resolution utilities. No other code in ModdableTimberborn needs to know how the container was obtained.

## Dependencies & patterns

- **Harmony** — `[HarmonyPatch]` / `[HarmonyPostfix]` attributes; the class must be picked up by a `Harmony.PatchAll()` call somewhere in the mod's entry point.
- **Bindito** — `IContainer` is the Bindito DI container interface native to Timberborn mods.
- **`ContainerCreator`** (Timberborn internal) — the patched type; lives outside this repo.
- No Bindito configurator or `[MultiBind]` registration is used here — the patch is self-contained and activates purely via Harmony attribute scanning.

## Notes / gotchas

- There is only one file in this folder; it is intentionally minimal — all logic lives in `ContainerRetriever` (Services folder), not here.
