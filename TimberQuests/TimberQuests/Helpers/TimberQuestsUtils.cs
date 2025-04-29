namespace TimberQuests.Helpers;

public static class TimberQuestsUtils
{

    public static bool HasMoreModLogs = AppDomain.CurrentDomain.GetAssemblies()
        .Any(a => a.GetName().Name == "MoreModLogs");

    internal static SingletonKey SaveKey = new("TimberQuests");

    public static void Log(Func<string> message)
    {
        if (HasMoreModLogs)
        {
            Debug.Log(message());
        }
    }

}
