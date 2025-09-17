using System.Diagnostics;

namespace TimberUi;

public static class TimberUiUtils
{
    public static readonly FrozenSet<string> LoadedAssemblyNames;
    public static readonly bool HasMoreModLogs;

    public const string SteamId = "1062090";

    static TimberUiUtils()
    {
        LoadedAssemblyNames = AppDomain.CurrentDomain.GetAssemblies()
            .Select(asm => asm.GetName().Name)
            .ToFrozenSet();

        HasMoreModLogs = LoadedAssemblyNames.Contains("MoreModLogs");
    }

    public static void LogVerbose(Func<string> msg)
    {
        if (!HasMoreModLogs) { return; }
        UnityEngine.Debug.Log(msg());
    }

    [Obsolete("Remember to remove this log after debugging")]
    public static void LogDev(object msg)
    {
        UnityEngine.Debug.Log(msg);
    }

    public static void DoNothing() { }

    public static void KillProcess()
    {
        var currentProcess = Process.GetCurrentProcess();
        currentProcess.Kill();
    }

    public static void Restart(int delaySeconds = 5)
    {
        string exe = Process.GetCurrentProcess().MainModule.FileName;
        string delayCmd, shell, shellArgs;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            delayCmd = $"timeout /T {delaySeconds} & start \"\" \"{exe}\"";
            shell = "cmd";
            shellArgs = $"/C {delayCmd}";
        }
        else
        {
            delayCmd = $"sleep {delaySeconds}; \"{exe}\"";
            shell = "/bin/bash";
            shellArgs = $"-c \"{delayCmd}\"";
        }

        Process.Start(new ProcessStartInfo(shell, shellArgs)
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            UseShellExecute = true
        });

        KillProcess();
    }

    public static AudioClip LoadAudioClipFrom(string filePath, string? name) => WavUtility.ToAudioClip(filePath, name: name);

    public static AudioClip LoadAudioClipFrom(byte[] bytes, string name) => WavUtility.ToAudioClip(bytes, name: name);

}
