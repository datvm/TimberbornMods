namespace ScientificProjects;

public static class ScientificProjectsUtils
{

    public static readonly bool HasMoreModLog = AppDomain.CurrentDomain.GetAssemblies().Any(q => q.GetName().Name == "MoreModLogs");

    public static void Log(Func<string> message)
    {
        if (HasMoreModLog)
        {
            Debug.Log(message());
        }
    }

}
