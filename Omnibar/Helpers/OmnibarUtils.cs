namespace Omnibar.Helpers;

public static class OmnibarUtils
{

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
        int span = pos[^1] - pos[0] + 1;   // distance that covers the match
        int compactness = kw.Length * 2 - span;

        // 2. Early‑match bonus – matches nearer the start are better
        int earlyBonus = text.Length - pos[0];

        // 3. Contiguity bonus – reward consecutive runs (e.g. “foo” in “foobar”)
        int contiguousRuns = 1;
        for (int i = 1; i < pos.Count; i++)
            if (pos[i] == pos[i - 1] + 1) contiguousRuns++;

        // 4. Length bonus
        var lenBonus = -text.Length;

        // Final score (tweak weights to taste)
        int score =
            compactness * 5         // compactness is most important
            + contiguousRuns * 10     // contiguous characters matter
            + earlyBonus              // slight bias for earlier matches
            + lenBonus;              // longer words are worse

        return new([.. pos], score);
    }

}
