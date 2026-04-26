namespace ModdableTimberborn.Helpers;

public static partial class CommonExtensions
{
    extension(ILoc t)
    {
        public string THours(float time, string? format = "F1") => t.T(UnitFormatter.HourUnitLocKey, time.ToString(format));
    }

    extension(NumericComparisonMode mode)
    {
        public char ToChar() => mode switch
        {
            NumericComparisonMode.Greater => '>',
            NumericComparisonMode.Less => '<',
            NumericComparisonMode.GreaterOrEqual => '≥',
            NumericComparisonMode.LessOrEqual => '≤',
            NumericComparisonMode.Equal => '=',
            NumericComparisonMode.NotEqual => '≠',
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    extension<TKey, TValue>(IEnumerable<TValue> list)
         where TKey : notnull
    {
        public FrozenDictionary<TKey, ImmutableArray<TValue>> GroupToDictionary(Func<TValue, TKey> key)

            => list.GroupToDictionary(key, q => q);

        public FrozenDictionary<TKey, ImmutableArray<TValue>> GroupToDictionary(
            Func<TValue, TKey> key,
            Func<List<TValue>, IEnumerable<TValue>> listFunc)
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
    }

    public static void SetGetOnlyProperty<T, TProp>(T obj, Expression<Func<T, TProp>> propertyExpression, TProp value)
    {
        if (propertyExpression.Body is not MemberExpression memberExpr)
        {
            throw new ArgumentException("The expression is not a valid member expression.", nameof(propertyExpression));
        }

        var propertyInfo = memberExpr.Member as PropertyInfo ?? throw new ArgumentException("The member is not a property.", nameof(propertyExpression));
        var backingFieldName = $"<{propertyInfo.Name}>k__BackingField";

        var fieldInfo = typeof(T).GetField(backingFieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Backing field '{backingFieldName}' not found in type '{typeof(T).FullName}'.");
        fieldInfo.SetValue(obj, value);
    }

    extension(GameObject obj)
    {
        public void RemoveComponent<T>(bool immediately = true) where T : Object
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
    }

    extension(BaseComponent component)
    {
        public TComp? GetComponentOrNull<TComp>()
            where TComp : BaseComponent
        {
            var result = component.GetComponent<TComp>();
            return result ? result : null;
        }
    }

    extension(BonusType t)
    {
        public BonusSpec ToBonusSpec(float multiplierDelta) => CreateBonusSpec(t.ToString(), multiplierDelta);
    }

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

    extension(ITimeTriggerFactory fac)
    {
        public ITimeTrigger CreateAndStart(Action action, float delayInDays)
        {
            var timer = fac.Create(action, delayInDays);
            timer.Resume();
            return timer;
        }
    }

}
