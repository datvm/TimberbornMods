# Code Style

Always use code block even for single conditional or loop statements. For example:
```cs
if (condition) { return; }

// NOT:
if (condition) return;
```

Always use latest C# features like pattern matching, switch expressions, and collection initializers. For example:
```cs
List<int> numbers = [];
MyType variable = new();

// NOT:
List<int> numbers = new();
MyType variable = new MyType();
```

Prefer no access modifier for lowest visibility. Do not use `_` to prefix fields. For example:
```cs
int field;
class MyClass {}

// NOT:
private int _field;
internal class MyClass {}
```

# Projects

When searching for references, check all the `*.code-workspace` files in the repository (note that Codex may be rooted one folder too deep when using VS Code Workspace, so search one upper folder) and see if they contain the project you are working on. If they do, search for references in all of those projects only. If not, search for references in that project only. For example:

- MoreHttpApi.code-workspace
- BeaverChronicles.code-workspace

# Tests

Keep test method names short and descriptive. Prefer names like `InitializeWithCorrectCharacterType`, `IgnoreWrongCharacterType`, or `ComputeSegments` over long Given/When/Then-style names.

Prefer focused tests with clear arrange/act/assert spacing. Use helpers or factories for repeated setup when Unity or Timberborn types require reflection or publicized DLL setup.

# Tools

Timberborn game source should be read from `ModdingAssetUpdater/out`, which contains decompiled game DLLs. When investigating game behavior, search and read that folder instead of decompiling assemblies on demand.

More exported Timberborn game data is stored at `D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources`. For example, the `Buildings` folder there contains game building blueprints that may be useful when comparing mod blueprints to vanilla behavior.

If `ModdingAssetUpdater/out` does not exist or is empty, ask the user to decompile the game source there first.

Game assemblies are publicized for mod projects. It is okay to reference publicized Timberborn types and members directly in code and Harmony patches instead of avoiding `internal` or private members with reflection-only approaches.

When converting grid coordinates or directions into Unity/world vectors, check `CoordinateSystem` and its namespace code first. It likely already has the conversion needed, so avoid adding local helpers like `ToUnityDirection` unless the built-in conversion does not fit.

## Timbermesh models (read / edit)

When you need to **read or edit** a `.timbermesh` model (node hierarchy, materials, meshes, doors/props, etc.), use the `tmesh2glb` tool from `Timbermesh2Blender` — do **not** try to parse the binary by hand.

Working directory for dumps/packs: **`D:\Temp\TimbermeshModel`**.

```powershell
# From repo root (or run the built tool / installed global tool `tmesh2glb`)
dotnet run --project Timbermesh2Blender -- dump -i <model-or-folder> -o D:\Temp\TimbermeshModel -f
# Edit the produced *.timbermesh.json
dotnet run --project Timbermesh2Blender -- pack -i D:\Temp\TimbermeshModel\<name>.timbermesh.json -o D:\Temp\TimbermeshModel -f
```

Notes:

- Default dump keeps vertex attributes as **base64** (lossless). Use `--decode-vertices` only when you need numeric position/normal/uv arrays.
- Prefer structural edits in JSON (delete/rename/reparent nodes, swap material names). Use **glb** export + Blender for real mesh sculpting.
- After packing, you can preview with `tmesh2glb glb -i ... -r <Resources> -o D:\Temp\TimbermeshModel -f`.
- Game Resources path: `D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources`.
