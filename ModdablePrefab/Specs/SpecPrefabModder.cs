global using Newtonsoft.Json;
global using System.Collections;
global using System.Collections.Frozen;
global using Timberborn.PrefabSystem;

namespace ModdablePrefab;

public class SpecPrefabModder(
    ISpecService specServ,
    FactionService factionService
) : ILoadableSingleton, IPrefabGroupProvider // IPrefabGroupProvider so its Load method run before PrefabGroupService.Load
{
    public static SpecPrefabModder? Instance { get; private set; }

    FrozenDictionary<Type, ImmutableArray<PrefabModderSpec>> moddersByTypes = null!;
    FrozenDictionary<Type, ImmutableArray<PrefabAddComponentSpec>> compAddersByTypes = null!;

    public void Load()
    {
        Dictionary<string, Type> typeCache = [];

        moddersByTypes = GroupSpecs<PrefabModderSpec>(typeCache);
        compAddersByTypes = GroupSpecs<PrefabAddComponentSpec>(typeCache);

        ConvertJsonStrings();
        PopulateAddComponentSpecTypes(typeCache);

        Instance = this;
    }

    FrozenDictionary<Type, ImmutableArray<T>> GroupSpecs<T>(Dictionary<string, Type> typeCache) where T : BasePrefabModSpec
    {
        var specs = specServ.GetSpecs<T>();
        try
        {
            if (specs is null || !specs.Any()) { return FrozenDictionary<Type, ImmutableArray<T>>.Empty; }
        }
        catch (Exception) // This awkward try-catch is needed because the game throws if there is no spec.
        {
            return FrozenDictionary<Type, ImmutableArray<T>>.Empty;
        }

        var factionId = factionService.Current.Id;

        Dictionary<Type, List<T>> byTypes = [];
        foreach (var spec in specs)
        {
            Debug.Log($"{spec} has faction: " + string.Join(", ", spec.Factions));
            if (spec.Factions.Length > 0 && !spec.Factions.Contains(factionId)) { continue; }

            var type = FindComponentType(spec.ComponentType, typeCache);

            if (!byTypes.TryGetValue(type, out var list))
            {
                byTypes[type] = list = [];
            }
            list.Add(spec);
        }
        return byTypes.ToFrozenDictionary(
            q => q.Key,
            q => q.Value.ToImmutableArray());
    }

    void ConvertJsonStrings()
    {
        foreach (var spec in moddersByTypes.SelectMany(q => q.Value))
        {
            spec.NormalizedNewValue = NormalizeJsonQuote(spec.NewValue);
        }
    }

    void PopulateAddComponentSpecTypes(Dictionary<string, Type> typeCache)
    {
        if (compAddersByTypes.Count == 0) { return; }

        foreach (var spec in compAddersByTypes.SelectMany(q => q.Value))
        {
            spec.AddComponentTypes = [.. spec.AddComponents.Select(q => FindComponentType(q, typeCache))];
        }
    }

    static Type FindComponentType(string name, Dictionary<string, Type> typeCache)
    {
        if (typeCache.TryGetValue(name, out var t)) { return t; }

        t = AccessTools.TypeByName(name)
            ?? throw new InvalidOperationException($"Cannot find the type {name}");

        if (typeof(BaseComponent).IsAssignableFrom(t))
        {
            typeCache[name] = t;
            return t;
        }
        else
        {
            throw new InvalidOperationException($"Type {name} is not a {nameof(BaseComponent)}");
        }
    }

    public void ModifyPrefab(GameObject prefab)
    {
        foreach (var comp in prefab.GetComponents<BaseComponent>())
        {
            AddPrefabComponents(comp, comp.GetType());
        }

        // Do not reuse the components list, as it may change during the above loop
        foreach (var comp in prefab.GetComponents<BaseComponent>())
        {
            ModifyPrefabValues(comp, comp.GetType());
        }
    }

    void AddPrefabComponents(BaseComponent prefab, Type type)
    {
        if (!compAddersByTypes.TryGetValue(type, out var specs) || specs.Length == 0)
        {
            return;
        }

        HashSet<Type>? addedComponents = []; // Only populate if needed once a prefab match

        foreach (var spec in specs)
        {
            if (!MatchPrefab(prefab, spec)) { continue; }

            // Fetch existing components
            addedComponents ??= [.. prefab.GameObjectFast.GetComponents<BaseComponent>().Select(q => q.GetType())];

            Debug.Log($"{nameof(ModdablePrefab)}: Adding components to prefab {prefab.name}");
            foreach (var comp in spec.AddComponentTypes)
            {
                if (addedComponents.Contains(comp))
                {
                    Debug.Log($"{nameof(ModdablePrefab)}: Component {comp} already exists in prefab {prefab.name}");
                    continue;
                }
                else
                {
                    Debug.Log($"{nameof(ModdablePrefab)}: Adding component {comp} to prefab {prefab.name}");
                    prefab.GameObjectFast.AddComponent(comp);
                }
            }
        }
    }

    void ModifyPrefabValues(BaseComponent prefab, Type type)
    {
        if (!moddersByTypes.TryGetValue(type, out var specs) || specs.Length == 0)
        {
            return;
        }

        foreach (var spec in specs)
        {
            if (!MatchPrefab(prefab, spec)) { continue; }

            Debug.Log($"{nameof(ModdablePrefab)}: Modifying component {spec.ComponentType} for prefab {prefab.name}");
            ModifyPrefabValue(prefab, spec);
        }
    }

    bool MatchPrefab(BaseComponent prefab, BasePrefabModSpec spec)
    {
        var names = spec.PrefabNames;
        if (names == default || names.Length == 0 || names.Contains(prefab.name)) { return true; }

        var prefabSpec = prefab.GetComponentFast<PrefabSpec>();
        if (prefabSpec is null) { return false; }

        return names.Any(prefabSpec.IsNamed);
    }

    static readonly JsonSerializerSettings jsonSettings = new()
    {
        ContractResolver = new PrivateJsonContractResolver(),
    };
    void ModifyPrefabValue(BaseComponent comp, PrefabModderSpec spec)
    {
        if (spec.ValuePath == "_")
        {
            JsonConvert.PopulateObject(spec.NormalizedNewValue, comp, new()
            {
                ContractResolver = new PrivateJsonContractResolver(),
            });
        }
        else
        {
            var (setValue, expectedType, original) = GetMember(comp, spec);

            var value = JsonConvert.DeserializeObject(spec.NormalizedNewValue, expectedType, jsonSettings);

            if (spec.AppendArray && value is IEnumerable enumerable && original is IEnumerable originalEnumerable)
            {
                // Use the JSON trick to concatenate two arrays
                value = JsonConvert.DeserializeObject(
                    JsonConvert.SerializeObject(
                        originalEnumerable.Cast<object>().Concat(enumerable.Cast<object>().Distinct()),
                        jsonSettings),
                    expectedType,
                    jsonSettings);
            }

            setValue(value);
        }
    }

    PrefabSpecReflectedMember GetMember(BaseComponent comp, PrefabModderSpec spec)
    {
        // Determine the property/field
        var path = spec.ValuePath.Split('.');
        Action<object?>? setValue = null;

        var currType = comp.GetType();
        object currObj = comp;
        var counter = 0;

        foreach (var p in path)
        {
            var isLast = counter == path.Length - 1;

            var members = currType.GetMember(p, AccessTools.all);

            if (members.Length == 0)
            {
                throw new InvalidOperationException($"Member {p} not found in {currType}. From spec path: {spec.ValuePath}");
            }

            var member = members.FirstOrDefault(q => q.MemberType is MemberTypes.Property or MemberTypes.Field)
                ?? throw new InvalidOperationException($"Member {p} is not a property or field in {currType}. From spec path: {spec.ValuePath}");

            if (member is PropertyInfo prop)
            {
                currType = prop.PropertyType;

                if (prop.SetMethod is null && !typeof(IEnumerable).IsAssignableFrom(currType))
                {
                    throw new InvalidOperationException($"Property {p} in {currType} is read-only. From spec path: {spec.ValuePath}");
                }

                if (isLast)
                {
                    var keepRef = currObj;
                    setValue = value => prop.SetValue(keepRef, value);
                }

                currObj = prop.GetValue(currObj);
            }
            else if (member is FieldInfo field)
            {
                currType = field.FieldType;

                if (isLast)
                {
                    var keepRef = currObj;
                    setValue = value => field.SetValue(keepRef, value);
                }

                currObj = field.GetValue(currObj);
            }
            else
            {
                throw new InvalidOperationException($"Member {p} is not a property or field in {currType}. From spec path: {spec.ValuePath}");
            }

            counter++;
        }

        if (setValue is null)
        {
            throw new InvalidOperationException($"No member found in {comp.GetType()} from spec path: {spec.ValuePath}");
        }

        return new(setValue, currType, currObj);
    }

    static string NormalizeJsonQuote(string input)
    {
        // Replace \' with a placeholder to preserve it during processing
        string placeholder = "__SINGLE_QUOTE__";
        input = input.Replace(@"\'", placeholder);

        // Replace all single quotes with double quotes
        input = input.Replace("'", "\"");

        // Replace the placeholder back with the single quote
        input = input.Replace(placeholder, "'");

        return input;
    }

    public IEnumerable<string> GetPrefabGroups() => [];
}

public readonly record struct PrefabSpecReflectedMember(Action<object?> SetValue, Type ExpectedType, object OriginalValue);