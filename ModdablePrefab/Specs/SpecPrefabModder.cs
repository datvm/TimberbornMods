global using Newtonsoft.Json;
global using System.Collections;
global using Timberborn.PrefabSystem;

namespace ModdablePrefab;

public class SpecPrefabModder(ISpecService specServ) : IPrefabModder, ILoadableSingleton
{
    public ImmutableHashSet<Type> PrefabTypes { get; private set; } = [];

    FrozenDictionary<Type, ImmutableArray<PrefabModderSpec>> moddersByTypes = null!;
    FrozenDictionary<Type, ImmutableArray<PrefabAddComponentSpec>> compAddersByTypes = null!;

    public void Load()
    {
        Dictionary<string, Type> typeCache = [];

        moddersByTypes = GroupSpecs<PrefabModderSpec>(typeCache);
        compAddersByTypes = GroupSpecs<PrefabAddComponentSpec>(typeCache);

        PopulateAddComponentSpecTypes(typeCache);

        PrefabTypes = [.. moddersByTypes.Keys, .. compAddersByTypes.Keys];
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

        Dictionary<Type, List<T>> byTypes = [];
        foreach (var spec in specs)
        {
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

    public void ModifyPrefab<T>(T prefab) where T : BaseComponent
    {
        var type = prefab.GetType(); // Don't use the generic type parameter here

        AddPrefabComponents(prefab, type);
        ModifyPrefabValues(prefab, type);
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

            Debug.Log($"{nameof(ModdablePrefab)}: Modifying prefab {prefab.name}");
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
    void ModifyPrefabValue(BaseComponent prefab, PrefabModderSpec spec)
    {
        var (setValue, expectedType, original) = GetMember(prefab, spec);

        var value = JsonConvert.DeserializeObject(spec.NewValue, expectedType, jsonSettings);

        if (spec.AppendArray && value is IEnumerable enumerable && original is IEnumerable originalEnumerable)
        {
            // Use the JSON trick to concatenate two arrays
            value = JsonConvert.DeserializeObject(
                JsonConvert.SerializeObject(
                    originalEnumerable.Cast<object>().Concat(enumerable.Cast<object>()),
                    jsonSettings),
                expectedType,
                jsonSettings);
        }

        setValue(value);
    }

    PrefabSpecReflectedMember GetMember(BaseComponent prefab, PrefabModderSpec spec)
    {
        // Determine the property/field
        var path = spec.ValuePath.Split('.');
        Action<object?>? setValue = null;

        var currType = prefab.GetType();
        object currObj = prefab;
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
            throw new InvalidOperationException($"No member found in {prefab.GetType()} from spec path: {spec.ValuePath}");
        }

        return new(setValue, currType, currObj);
    }

}

public readonly record struct PrefabSpecReflectedMember(Action<object?> SetValue, Type ExpectedType, object OriginalValue);