namespace Timberborn.Localization;

public static partial class UiBuilderExtensions
{

    public static string T(this string key, ILoc t)
    {
        return t.T(key);
    }

    public static string T<T1>(this string key, ILoc t, T1 t1)
    {
        return t.T(key, t1);
    }

    public static string T<T1, T2>(this string key, ILoc t, T1 t1, T2 t2)
    {
        return t.T(key, t1, t2);
    }

    public static string T<T1, T2, T3>(this string key, ILoc t, T1 t1, T2 t2, T3 t3)
    {
        return t.T(key, t1, t2, t3);
    }

    public static string TFormat(this string key, ILoc t, params object[] args)
    {
        return string.Format(t.T(key), args);
    }

}
