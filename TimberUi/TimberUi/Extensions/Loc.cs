namespace Timberborn.Localization;

public static partial class UiBuilderExtensions
{

    static string? Yes, No, OK, Cancel, None;
    static string?[] Priorities = new string?[5];

    extension(string key)
    {
        public string T(ILoc t) => t.T(key);
        public string T<T1>(ILoc t, T1 t1) => t.T(key, t1);
        public string T<T1, T2>(ILoc t, T1 t1, T2 t2) => t.T(key, t1, t2);
        public string T<T1, T2, T3>(ILoc t, T1 t1, T2 t2, T3 t3) => t.T(key, t1, t2, t3);
        public string TFormat(ILoc t, params object[] args) => string.Format(t.T(key), args);
    }

    extension(ILoc t)
    {
        public string TYes() => Yes ??= t.T("Core.Yes");
        public string TNo() => No ??= t.T("Core.No");
        public string TOK() => OK ??= t.T("Core.OK");
        public string TCancel() => Cancel ??= t.T("Core.Cancel");
        public string TYesNo(bool value) => value ? t.TYes() : t.TNo();
        public string TNone() => None ??= t.T("Inventory.NothingSelected");
        public string T(Priority priority) => priority.T(t);
    }

    extension(Priority priority)
    {
        public string T(ILoc t) => Priorities[(int)priority] ??= t.T($"Priorities.{priority}");
    }

}
