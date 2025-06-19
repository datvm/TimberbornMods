namespace MacroManagement.Models;

public readonly record struct MultiSelectInfo(ImmutableArray<PrefabSpec> Items, MMComponent Placeholder);
