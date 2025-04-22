global using System.Windows.Forms;

namespace TImprove4Mods.Services;

public class FileDialogService
{

    public static readonly bool IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
    static string GetDefaultFolder() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Timberborn\PlayerData");

    const string WindowsFormsTextFilter = "Text Files (*.txt)|*.txt";
    public string? OpenFile()
    {
        if (IsWindows)
        {
            return OpenWindowsFile();
        }

        OpenFileDialog diag = new()
        {
            Filter = WindowsFormsTextFilter,
        };

        return diag.ShowDialog() == DialogResult.OK ? diag.FileName : null;
    }

    public string? SaveFile()
    {
        if (IsWindows)
        {
            return SaveWindowsFile();
        }

        SaveFileDialog diag = new()
        {
            Filter = WindowsFormsTextFilter,
            InitialDirectory = GetDefaultFolder(),
        };
        return diag.ShowDialog() == DialogResult.OK ? diag.FileName : null;
    }

    const string WindowsTextFilter = "Text Files (*.txt)\0*.txt\0\0";
    string? OpenWindowsFile()
    {
        return WindowsFileDialogs.ShowOpenFileDialog(WindowsTextFilter);
    }

    string? SaveWindowsFile()
    {
        return WindowsFileDialogs.ShowSaveFileDialog(WindowsTextFilter, GetDefaultFolder());
    }

}
