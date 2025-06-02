using System.Diagnostics;

namespace TimberUi;

public static class TimberUiUtils
{
    public const string SteamId = "1062090";

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

}
