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

Prefer no access modifier for lowest visibility. Do not use _ to prefix fields. For example:
```cs
int filed;
class MyClass {}

// NOT:
private int _field;
internal class MyClass {}
```

# Projects

When searching for references, check all the `*.code-workspace` files in the repository (note that Codex may be rooted one folder too deep when using VS Code Workspace, so search one upper folder) and see if they contain the project you are working on. If they do, search for references in all of those projects only. If not, search for references in that project only. For example:

- MoreHttpApi.code-workspace
- BeaverChronicles.code-workspace