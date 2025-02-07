// Check for existing process
using System.Diagnostics;

const string SteamId = "1062090";

var process = Process.GetProcesses()
    .FirstOrDefault(q => q.ProcessName == "Timberborn");

if (process is not null)
{
    Console.WriteLine("Process found, killing...");
    process.Kill();
}

Console.WriteLine("Starting");
Process.Start(new ProcessStartInfo("steam://launch/" + SteamId)
{
    UseShellExecute = true,
});