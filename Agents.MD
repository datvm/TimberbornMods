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

If `ModdingAssetUpdater/out` does not exist or is empty, ask the user to decompile the game source there first.
