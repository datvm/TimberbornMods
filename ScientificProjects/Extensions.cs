﻿namespace System;

public static class ResearchProjectsModExtensions
{

    public static TInstance CreateInstance<TBuff, TInstance>(this TBuff buff, IEnumerable<ScientificProjectInfo> projects, out TInstance result)
        where TBuff : CommonProjectsBuff
        where TInstance : CommonProjectBuffInstance<TBuff>, new()
    {
        result = buff.buffs.CreateBuffInstance<TBuff, TInstance, IEnumerable<ScientificProjectInfo>>(buff, projects);
        return result;
    }

}

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

    public static T SetFlex101<T>(this T element) where T : VisualElement
    {
        element.style.flexGrow = 1;
        element.style.flexShrink = 0;
        element.style.flexBasis = 1;
        return element;
    }

}
