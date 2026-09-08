namespace Omnibar.Helpers;

public static class OmnibarUtils
{
    public const char MathTrigger = '=';
    public const char CommandTrigger = '/';
    public static readonly ImmutableHashSet<char> SpecialTriggers =
        new HashSet<char>([MathTrigger, CommandTrigger]).ToImmutableHashSet();

    public static bool IsCommand(this string filter) => SpecialTriggers.Contains(filter[0]);

    public static IReadOnlyList<OmnibarFilteredItem> StandardFilter<T>(IEnumerable<T> items, string filter, Func<T, string> textFunc)
        where T : IOmnibarItem
    {
        return [.. items
            .Select(q => new OmnibarFilteredItem(q, MatchText(filter, textFunc(q)) ?? default))
            .Where(q => q.Match != default)];
    }

    public static FuzzyMatchResult? MatchText(string kw, string text)
    {
        if (string.IsNullOrEmpty(text)) { return null; }

        text = text.ToLower();

        if (kw == text)
        {
            return new([.. Enumerable.Range(0, kw.Length)], int.MaxValue);
        }

        var pos = new List<int>();
        var ki = 0;

        for (int ti = 0; ti < text.Length && ki < kw.Length; ti++)
        {
            if (text[ti] == kw[ki])
            {
                pos.Add(ti);
                ki++;
            }
        }

        if (ki != kw.Length) { return null; }

        // ‑‑ Relevance score ----------------------------------------------------
        // 1. Compactness bonus – closer characters = higher score
        int span = pos[pos.Count - 1] - pos[0] + 1;   // distance that covers the match
        int compactness = kw.Length * 2 - span;

        // 2. Early‑match bonus – matches nearer the start are better
        int earlyBonus = (int)((text.Length - pos[0]) * (kw.Length / (double)text.Length));

        // 3. Contiguity bonus – reward consecutive runs (e.g. “foo” in “foobar”)
        int contiguousRuns = 1;
        for (int i = 1; i < pos.Count; i++)
            if (pos[i] == pos[i - 1] + 1) contiguousRuns++;

        // 4. Length bonus
        var lenBonus = -text.Length;

        // Final score (tweak weights to taste)
        int score =
            compactness * 5
            + contiguousRuns * 10
            + earlyBonus * 10
            + lenBonus;

        return new([.. pos], score);
    }

    public static GoodAmountSpec Multiply(this GoodAmountSpec spec, int quantity)
        => spec with { _amount = spec._amount * quantity, };

    public static IEnumerable<GoodAmountSpec> Multiply<T>(this T collection, int quantity) 
        where T: IEnumerable<GoodAmountSpec>
        => collection.Select(q => q.Multiply(quantity));

    public static string GetOmnibarItemHotkey(this ILoc t, string hotkey, string locKey) =>
        $"{hotkey.Bold().Color(TimberbornTextColor.Solid)}: {t.T(locKey)}";

    public static bool IsLocked(this BlockObjectTool tool) => tool.Locker is not null;
    public static bool IsLocked(this TodoListEntryBuilding building) => 
        building.BuildingTool?.IsLocked() == true;

}
