namespace System.Collections.Generic;

public static class ImmutableHelper
{

    public static FrozenSet<T> CreateFrozenSet<T>(params IEnumerable<T> items) => items.ToFrozenSet();


}
