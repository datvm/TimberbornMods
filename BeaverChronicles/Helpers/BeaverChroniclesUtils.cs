namespace BeaverChronicles.Helpers;

public static class BeaverChroniclesUtils
{
    public const string Science = "Science";

    public const CharacterType AllCharactersEnum = CharacterType.AdultBeaver | CharacterType.ChildBeaver | CharacterType.Bot;
    public static readonly ImmutableArray<CharacterType> AllCharacters = [ CharacterType.AdultBeaver, CharacterType.ChildBeaver, CharacterType.Bot, ];

    public static readonly ImmutableArray<EventTriggerSource> AllTriggerSources
        = [.. Enum.GetValues(typeof(EventTriggerSource)).Cast<EventTriggerSource>()];

    public static void Log(string msg) => Debug.Log($"[{nameof(BeaverChronicles)}] {msg}");

    public static void LogVerbose(Func<string> msgFunc) => TimberUiUtils.LogVerbose(() => $"[{nameof(BeaverChronicles)}] {msgFunc()}");

    public static bool Chance(float chance)
    {
        var value = Random.value;
        var result = value < chance;

        LogVerbose(() => $"Chance check: {chance:P2} vs {value:P2} => {(result ? "Success" : "Failure")}");
        return result;
    }

    extension(IChronicleEvent ev)
    {
        public bool IsMiniEvent => ev is IMiniChronicleEvent;
    }

    extension<T>(IReadOnlyCollection<T> collections)
    {
        public bool EmptyOrAny(Predicate<T> fn) => collections.Count == 0 || collections.Any(c => fn(c));
        public bool EmptyOrContains(T item) => collections.Count == 0 || collections.Contains(item);
    }

    extension<T>(T el) where T : VisualElement
    {

        public T HighlightRed() => el.HighlightAll(TimberbornTextColor.Red);
        public T HighlightGreen() => el.HighlightAll(TimberbornTextColor.Green);

        public T HighlightAll(GoodModifier mod) => el.HighlightAll(mod switch
        {
            GoodModifier.Positive => TimberbornTextColor.Green,
            GoodModifier.Negative => TimberbornTextColor.Red,
            GoodModifier.Neutral or _ => null,
        });

        public T HighlightAll(TimberbornTextColor? color)
        {
            if (color is null) { return el; }

            if (el is TextElement lbl)
            {
                lbl.text = lbl.text.Color(color.Value);
            }

            foreach (var child in el.Children())
            {
                child.HighlightAll(color);
            }

            return el;
        }
    }

    extension(string str)
    {
        public string HighlightRed() => str.Color(TimberbornTextColor.Red);
        public string HighlightGreen() => str.Color(TimberbornTextColor.Green);
    }

    extension(ILoc t)
    {
        public string TNoDc() => t.T("LV.BCEv.DisabledNoDc");
        public string TNoSpawn() => t.T("LV.BCEv.DisabledNoSpawnLocation");

        public string TEventContent(string evId) => t.T(ChronicleEventUIHelper.GetDefaultContentLoc(evId));
        public string TEventChoice(string evId, int index) => t.T($"LV.BCEv.{evId}.C{index + 1}");
        public string TEventChoiceNote(string evId, int index) => t.T($"LV.BCEv.{evId}.C{index + 1}N");
        public string TEventOutcome(string evId, int index) => t.T($"LV.BCEv.{evId}.O{index + 1}");
        public string TUnknownConsequences() => t.T("LV.BCEv.UnknownConsequences");
    }

    extension(IEventTriggerParameters p)
    {

        public bool IsParameter<T>() => p is IEventTriggerParameters<T>
            || p.DataObject is ITriggerParameterWith<T>;
        public int GetParameterWeight<T>() => p.IsParameter<T>() ? int.MaxValue : 0;

        public T GetParameter<T>() => p is IEventTriggerParameters<T> tp
            ? tp.Data
            : p.DataObject is ITriggerParameterWith<T> provider
                ? provider.Parameter
                : throw new InvalidOperationException($"Unexpected parameters type: {p.GetType().FullName}, instead of {typeof(EventTriggerParameter<T>)} or {typeof(ITriggerParameterWith<T>)}");

        public T? GetParameterOrDefault<T>() => p is IEventTriggerParameters<T> tp
            ? tp.Data
            : p.DataObject is ITriggerParameterWith<T> provider
                ? provider.Parameter
                : default;

    }

    extension(string? str)
    {
        [return: NotNullIfNotNull(nameof(str))]
        public string? Format(params object[] args) => str is null ? null : string.Format(str, args);

        [return: NotNullIfNotNull(nameof(str))]
        public string? CenterMixed() => str is null ? null : "[[C]]" + str;

        [return: NotNullIfNotNull(nameof(str))]
        public string? UncenterMixed() => str?.Replace("[[C]]", "");

    }

    extension(BaseComponent comp)
    {
        public StatusDescriptionComponent GetStatusDescription() => comp.GetComponent<StatusDescriptionComponent>();
        public NeedManager GetNeedManager() => comp.GetComponent<NeedManager>();
    }

    extension(IReadOnlyList<object> list)
    {

        public IEnumerable<GoodAmount> ToGoods(int startIndex)
        {
            for (int i = startIndex; i + 1 < list.Count; i += 2)
            {
                yield return new((string)list[i], (int)list[i + 1]);
            }
        }

    }

    extension(IEnumerable<GoodAmount> goods)
    {

        public string ToMixedText() => string.Join(", ", goods.Select(g => $"[[good:{g.GoodId}:{g.Amount}]]"));

    }

    extension(ConditionType c)
    {
        public bool Evaluate<T>(IReadOnlyList<T> values, Predicate<T> fn)
        {
            if (values.Count == 0 || c == ConditionType.Never) { return false; }

            if (values.Count == 1)
            {
                return fn(values[0]);
            }

            return c switch
            {
                ConditionType.Any => values.FastAny(v => fn(v)),
                ConditionType.All => values.FastAll(v => fn(v)),
                _ => throw new InvalidOperationException($"Unexpected condition type: {c}"),
            };
        }
    }

    extension(string str)
    {
        public bool IsScience => str == Science;
    }

}
