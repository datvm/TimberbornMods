namespace ConveyorBelt.Models;

public static class BeltRates
{
    public static int Capacity { get; private set; } = 5;
    public static float Spacing { get; private set; } = 1f / 5f;
    public const float End = 1f;

    public static void Use(int capacity)
    {
        if (capacity < 1)
        {
            throw new InvalidOperationException("Conveyor belt capacity must be at least 1.");
        }

        Capacity = capacity;
        Spacing = 1f / capacity;
    }

    public static float Delta(float itemsPerHour, float hoursPerTick)
        => itemsPerHour * hoursPerTick;
}
