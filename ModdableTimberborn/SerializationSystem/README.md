# SerializationSystem

## Purpose

Provides JSON serialization and deserialization support for Timberborn's `SerializedObject` type using Newtonsoft.Json. This folder exists because Timberborn's own `SerializedObject` is a structured property-bag that doesn't map cleanly to standard JSON converters, so a custom converter and convenience extension methods are needed to bridge the two.

## Key types

- **`SerializedObjectJsonConverter`** (`ModdableTimberborn.SerializationSystem`) — A singleton `JsonConverter<SerializedObject>` that handles recursive read/write between `SerializedObject` trees and raw JSON. Supports nested objects, arrays, and the primitive types int, float, bool, string, and null. Throws on unsupported token types (no silent failure).
- **`ModdableTimberbornSerializationExtensions`** (`Timberborn.SerializationSystem`) — A static class (using C# extension method syntax) that surfaces three convenience helpers: `DeserializeSerializedObject(string json)` (static), `SerializeToJson()` (on `SerializedObject`), and `DeserializeTo<T>()` (round-trips a `SerializedObject` through JSON into any target type `T`).

## How it fits together

`SerializedObjectJsonConverter` is a singleton wired into a shared `JsonSerializerSettings` instance held by `ModdableTimberbornSerializationExtensions`. All three public helpers use those same settings, so the converter is always active. The typical consumer flow is:

1. Receive or construct a `SerializedObject` from Timberborn's serialization pipeline.
2. Call `.SerializeToJson()` to get a plain JSON string (e.g. for persistence, logging, or inter-mod data exchange).
3. Call `DeserializeSerializedObject(json)` to reconstruct from a stored string, or `.DeserializeTo<T>()` to map directly into a strongly-typed C# object.

`DeserializeTo<T>()` works by first serializing the `SerializedObject` back to JSON, then deserializing into `T` — a two-pass round-trip that lets callers use standard Newtonsoft deserialization without hand-coding property lookups.

## Dependencies & patterns

- **Newtonsoft.Json** (`JsonConvert`, `JsonConverter<T>`, `JToken`/`JProperty`) — the only external dependency in this folder.
- **Timberborn game assemblies** — `SerializedObject` and its `Properties()` / `GetSerialized()` API are from the game, not defined here.
- No DI registration, no Harmony patches, no Unity dependencies in this folder.
- The two files sit in different namespaces on purpose: the converter lives in `ModdableTimberborn.SerializationSystem` (mod-owned), while the extensions live in `Timberborn.SerializationSystem` (game namespace) so they appear alongside the type they extend without requiring a `using` statement at the call site.

## Notes / gotchas

- `DeserializeTo<T>()` performs a serialize-then-deserialize round-trip, which doubles JSON processing. Callers passing custom `JsonSerializerSettings` to `DeserializeTo<T>()` should be aware that those settings do **not** include `SerializedObjectJsonConverter` — only the outer call chain uses it.
- The `extension(SerializedObject obj)` syntax in `ModdableTimberbornSerializedObjectExtensions.cs` is the C# 13 "extension member" preview feature, not classic `this` extension methods. Requires a compiler/SDK that supports it.
