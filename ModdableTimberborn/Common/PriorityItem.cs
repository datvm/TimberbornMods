namespace ModdableTimberborn.Common;

public record PriorityItem<T>(T Item, int Priority) : IComparable<PriorityItem<T>>
{
    public int CompareTo(PriorityItem<T>? other)
    {
        var result = Priority.CompareTo(other?.Priority ?? 0);

        return result == 0
            ? Item?.GetHashCode().CompareTo(other?.Item?.GetHashCode() ?? 0) ?? 0
            : result;
    }
}
