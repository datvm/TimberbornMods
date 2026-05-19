# Copilot Instructions

## General Guidelines
- When validating changes, build only the projects containing modified files instead of building the whole solution.
- Do not run `dotnet build` when changes are limited to JavaScript and CSS files only.

## Code Style
- Always use code blocks even for single conditional or loop statements. For example:
```cs
if (condition) { return; }

// NOT:
if (condition) return;
```
- Always use the latest C# features like pattern matching, switch expressions, and collection initializers. For example:
```cs
List<int> numbers = [];
MyType variable = new();

// NOT:
List<int> numbers = new();
MyType variable = new MyType();
```
- Prefer no access modifier for lowest visibility. Do not use _ to prefix fields. For example:
```cs
int field;
class MyClass {}

// NOT:
private int _field;
internal class MyClass {}
