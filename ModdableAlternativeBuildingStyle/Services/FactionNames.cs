namespace ModdableAlternativeBuildingStyle.Services;

// Node names that differ only by the faction token. Confirmed on blueprints and decompressed models:
// Folktails ↔ IronTeeth anywhere in the name, and FT ↔ IT as its own token
// (#HandFT, #display_FT, #display1x1_FT, #Slot#Spot.FT, #SharpenerFTAnimated, #Scratcher_IT_Animated).
static class FactionNames
{
    public static string? Counterpart(string name)
    {
        var swapped = Swap(name);
        return swapped == name ? null : swapped;
    }

    public static string Swap(string name)
    {
        var faction = SwapPair(name, "Folktails", "IronTeeth");
        return SwapPair(faction, "FT", "IT");
    }

    static string SwapPair(string name, string left, string right)
    {
        const string mark = "\u0001";
        if (!name.Contains(left) && !name.Contains(right))
        {
            return name;
        }

        return name.Replace(left, mark).Replace(right, left).Replace(mark, right);
    }
}
