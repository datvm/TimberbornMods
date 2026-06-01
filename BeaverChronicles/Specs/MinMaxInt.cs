namespace BeaverChronicles.Specs;

public static class MinMax
{
    public static readonly MinMaxInt Min1 = new(1);
    public static readonly MinMaxInt Zero = new(0, 0);
    public static readonly MinMaxInt Exact1 = new(1, 1);
}

public record MinMaxInt(int? Min = null, int? Max = null)
{

    public bool Evaluate(int value)
    {
        if (Min.HasValue && value < Min.Value) { return false; }
        if (Max.HasValue && value > Max.Value) { return false; }
        return true;
    }

    public bool Evaluate(IEnumerable<bool> yields)
    {
        int? min = Min, max = Max;
        bool hasMin = min.HasValue, hasMax = max.HasValue;
        if (!hasMin && !hasMax) { return true; }

        var counter = 0;
        foreach (var y in yields)
        {
            if (!y) { continue; }

            counter++;

            if (hasMax && counter > max!.Value) { return false; }
            if (hasMin && !hasMax && counter >= min!.Value) { return true; }
        }

        return Evaluate(counter);
    }

}
