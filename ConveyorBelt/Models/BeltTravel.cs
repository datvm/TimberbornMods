namespace ConveyorBelt.Models;

public readonly record struct BeltStop(int Index, float Position);

public static class BeltTravel
{
    public static float Distance(float itemsPerHour, float hours)
        => itemsPerHour * hours;

    public static float Hours(float itemsPerHour, float distance)
    {
        if (itemsPerHour <= 0f)
        {
            return 0f;
        }

        return distance / itemsPerHour;
    }

    // Empty belts. Leftover hours are spent one belt at a time. A belt is crossed when the
    // distance is at least its length; what remains is asked of the next belt.
    public static BeltStop Cross(IReadOnlyList<float> itemsPerHour, float leftoverHours)
    {
        var hours = leftoverHours;
        for (var i = 0; i < itemsPerHour.Count; i++)
        {
            var distance = Distance(itemsPerHour[i], hours);
            var last = i == itemsPerHour.Count - 1;
            if (distance < BeltRates.End || last)
            {
                var position = distance < BeltRates.End ? distance : BeltRates.End;
                return new(i, position);
            }

            hours -= Hours(itemsPerHour[i], BeltRates.End);
            if (hours < 0f)
            {
                hours = 0f;
            }
        }

        return new(0, 0f);
    }
}
