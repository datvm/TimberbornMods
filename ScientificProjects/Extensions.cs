namespace System;

internal static class ModExtensions
{

    public static string T(this string s, ILoc t)
    {
        return t.T(s);
    }

    public static string T<T1>(this string s, ILoc t, T1 param1)
    {
        return t.T(s, param1);
    }

    public static string T<T1, T2>(this string s, ILoc t, T1 param1, T2 param2)
    {
        return t.T(s, param1, param2);
    }

    public static string T<T1, T2, T3>(this string s, ILoc t, T1 param1, T2 param2, T3 param3)
    {
        return t.T(s, param1, param2, param3);
    }

    public static T SetFlexShrink<T>(this T element, float value = 0) where T : VisualElement
    {
        element.style.flexShrink = value;
        return element;
    }

}
