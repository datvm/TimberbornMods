
namespace BeaverChronicles.Helpers;

public static class BeaverChroniclesUtils
{
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

    extension(Configurator config)
    {

        public Configurator BindAllEvents(Assembly? assembly = default)
        {
            assembly ??= Assembly.GetCallingAssembly();

            foreach (var t in assembly.GetTypes())
            {
                if (typeof(IChronicleEvent).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && t.IsClass)
                {
                    config.BindSingleton(t);
                    config.MultiBind(typeof(IChronicleEvent), t, true);
                }
            }

            return config;
        }

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

        public bool IsParameter<T>() => p is EventTriggerParameter<T>;
        public int GetParameterWeight<T>() => p is EventTriggerParameter<T> ? int.MaxValue : 0;

        public T GetParameter<T>() => p is EventTriggerParameter<T> tp
            ? tp.Data
            : throw new InvalidOperationException($"Unexpected parameters type: {p.GetType().FullName}, instead of {typeof(EventTriggerParameter<T>)}");

        public T? GetParameterOrDefault<T>() => p is EventTriggerParameter<T> tp ? tp.Data : default;

    }

    extension (string? str)
    {
        [return: NotNullIfNotNull(nameof(str))]
        public string? Format(params object[] args) => str is null ? null : string.Format(str, args);

        [return: NotNullIfNotNull(nameof(str))]
        public string? CenterMixed() => str is null ? null : "[[C]]" + str;

        [return: NotNullIfNotNull(nameof(str))]
        public string? UncenterMixed() => str?.Replace("[[C]]", "");

    }

    extension (BaseComponent comp)
    {
        public StatusDescriptionComponent GetStatusDescription() => comp.GetComponent<StatusDescriptionComponent>();
        public NeedManager GetNeedManager() => comp.GetComponent<NeedManager>();
    }

}
