namespace ConveyorBelt.Models;

public static class BeltRates
{
    public const int Capacity = 5;
    public const float Spacing = 1f / Capacity;
    public const float End = 1f;

    public static float Delta(float itemsPerHour, float hoursPerTick)
        => Spacing * itemsPerHour * hoursPerTick;
}
