global using System.Collections;
global using Timberborn.PrefabSystem;
global using Newtonsoft.Json;

namespace ModdablePrefab;

public class SpecPrefabModder(ISpecService specServ) : IPrefabModder, ILoadableSingleton
{
    public ImmutableHashSet<Type> PrefabTypes { get; private set; } = [];

    FrozenDictionary<Type, ImmutableArray<PrefabModderSpec>> specsByTypes = null!;

    public void Load()
    {
        var specs = specServ.GetSpecs<PrefabModderSpec>();
        try
        {
            if (specs is null || !specs.Any()) { return; }
        }
        catch (Exception)
        {
            return;
        }
        
        
        Dictionary<string, Type> typeCache = [];
        Dictionary<Type, List<PrefabModderSpec>> specsByTypes = [];

        foreach (var spec in specs)
        {
            if (!typeCache.TryGetValue(spec.ComponentType, out var type))
            {
                typeCache[spec.ComponentType] = type = FindComponentType(spec.ComponentType);
            }

            if (!specsByTypes.TryGetValue(type, out var list))
            {
                specsByTypes[type] = list = [];
            }

            list.Add(spec);
        }

        this.specsByTypes = specsByTypes.ToFrozenDictionary(
            q => q.Key,
            q => q.Value.ToImmutableArray());
        PrefabTypes = [.. specsByTypes.Keys];
    }

    static Type FindComponentType(string name)
    {
        var t = AccessTools.TypeByName(name)
            ?? throw new InvalidOperationException($"Cannot find the type {name}");

        return typeof(BaseComponent).IsAssignableFrom(t)
            ? t
            : throw new InvalidOperationException($"Type {name} is not a {nameof(BaseComponent)}");
    }

    public void ModifyPrefab<T>(T prefab) where T : BaseComponent
    {
        var type = prefab.GetType(); // Don't use the generic type parameter here
        if (!specsByTypes.TryGetValue(type, out var specs))
        {
            throw new InvalidOperationException($"No specs found for {type}. This should not happen.");
        }

        foreach (var spec in specs)
        {
            if (MatchPrefab(prefab, spec))
            {
                Debug.Log($"{nameof(ModdablePrefab)}: Modifying prefab {prefab.name}");

                ModifyPrefab(prefab, spec);
            }
        }
    }
    
    bool MatchPrefab(BaseComponent prefab, PrefabModderSpec spec)
    {
        var names = spec.PrefabNames;
        if (names == default || names.Length == 0 || names.Contains(prefab.name)) { return true; }

        var buildingSpec = prefab.GetComponentFast<PrefabSpec>();
        if (buildingSpec is null) { return false; }

        return names.Any(buildingSpec.IsNamed);
    }

    static readonly JsonSerializerSettings jsonSettings = new()
    {
        ContractResolver = new PrivateJsonContractResolver(),
    };

    void ModifyPrefab(BaseComponent prefab, PrefabModderSpec spec)
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