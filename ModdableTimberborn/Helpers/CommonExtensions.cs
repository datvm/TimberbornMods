namespace ModdableTimberborn.Helpers;

public static partial class CommonExtensions
{

    public static FrozenDictionary<TKey, ImmutableArray<TValue>> GroupToDictionary<TKey, TValue>(this IEnumerable<TValue> list, Func<TValue, TKey> key)
        where TKey : notnull
        => GroupToDictionary(list, key, q => q);

    public static FrozenDictionary<TKey, ImmutableArray<TValue>> GroupToDictionary<TKey, TValue>(
        this IEnumerable<TValue> list, 
        Func<TValue, TKey> key,
        Func<List<TValue>, IEnumerable<TValue>> listFunc)
        where TKey : notnull
    {
        Dictionary<TKey, List<TValue>> dict = [];
        foreach (var item in list)
        {
            var k = key(item);
            if (!dict.TryGetValue(k, out var lst))
            {
                dict[k] = lst = [];
            }
            lst.Add(item);
        }

        return dict.ToFrozenDictionary(q => q.Key, values => listFunc(values.Value).ToImmutableArray());
    }

    public static void SetGetOnlyProperty<T, TProp>(T obj, Expression<Func<T, TProp>> propertyExpression, TProp value)
    {
        if (propertyExpression.Body is not MemberExpression memberExpr)
        {
            throw new ArgumentException("The expression is not a valid member expression.", nameof(propertyExpression));
        }

        var propertyInfo = memberExpr.Member as PropertyInfo ?? throw new ArgumentException("The member is not a property.", nameof(propertyExpression));
        var backingFieldName = $"<{propertyInfo.Name}>k__BackingField";
        
        var fieldInfo = typeof(T).Field(backingFieldName) ?? throw new InvalidOperationException($"Backing field '{backingFieldName}' not found in type '{typeof(T).FullName}'.");
        fieldInfo.SetValue(obj, value);
    }

    public static void RemoveComponent<T>(this GameObject obj, bool immediately = true) where T : Object
    {
        var comp = obj.GetComponent<T>();
        if (!comp)
        {
            Debug.LogWarning($"Trying to remove component {typeof(T).Name} but it does not exist on {obj.name}");
            return;
        }

        if (immediately)
        {
            Object.DestroyImmediate(comp);
        }
        else
        {
            Object.Destroy(comp);
        }
    }

    public static TComp? GetComponentOrNull<TComp>(this BaseComponent component)
        where TComp : BaseComponent
    {
        var result = component.GetComponent<TComp>();
        return result ? result : null;
    }

    public static BonusSpec ToBonusSpec(this BonusType t, float multiplierDelta) => CreateBonusSpec(t.ToString(), multiplierDelta);
    
    public static BonusSpec CreateBonusSpec(string id, float multiplierDelta) => new()
    {
        Id = id,
        MultiplierDelta = multiplierDelta,
    };

    extension(GameModeSpec spec)
    {
        public GameDifficultyEnum GetDifficultyEnum() => spec.DisplayNameLocKey switch
        {
            "NewGameConfigurationPanel.Easy" => GameDifficultyEnum.Easy,
            "NewGameConfigurationPanel.Normal" => GameDifficultyEnum.Medium,
            "NewGameConfigurationPanel.Hard" => GameDifficultyEnum.Hard,
            _ => GameDifficultyEnum.Custom,
        };
    }

    

}