namespace TimberUi.Services.SystemFileDialog;

public class WindowsSystemFileDialogService : ISystemFileDialogService
{
    public string? ShowOpenFileDialog(string? filter = default)
    {
        return WindowsFileDialogs.ShowOpenFileDialog(ParseFilter(filter));
    }

    public string? ShowSaveFileDialog(string? filter = default)
    {
        return WindowsFileDialogs.ShowSaveFileDialog(ParseFilter(filter));
    }

    static string ParseFilter(string? input)
    {
        if (input is null)
        {
            return WindowsFileDialogs.DefaultFilter;
        }
        else
        {
            var parts = input.Split(';');
            var filter = string.Join(';', parts.Select(q => "*" + q));

            return $"{input}\0{filter}\0\0";
        }
    }
}

public class WindowsFileDialogs
{
    public const string DefaultFilter = "All Files\0*.*\0\0";

    [DllImport("comdlg32.dll", CharSet = CharSet.Auto)]
static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

    [DllImport("comdlg32.dll", CharSet = CharSet.Auto)]
static extern bool GetSaveFileName([In, Out] OpenFileName ofn);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class OpenFileName
    {
        public int lStructSize = Marshal.SizeOf(typeof(OpenFileName));
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string? lpstrFilter;
        public string? lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public string? lpstrFile;
        public int nMaxFile = 256;
        public string? lpstrFileTitle;
        public int nMaxFileTitle = 64;
        public string? lpstrInitialDir;
        public string? lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public string? lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string? lpTemplateName;
    }

    public static string? ShowOpenFileDialog(string filter = "All Files\0*.*\0\0", string initialDir = "")
    {
        OpenFileName ofn = new()
        {
            lpstrFile = new string(new char[256]),
            lpstrFileTitle = new string(new char[64]),
            lpstrFilter = filter,
            lpstrInitialDir = initialDir,
            nFilterIndex = 1,
            Flags = 0x00000008 | 0x00080000
        };

        return GetOpenFileName(ofn) ? ofn.lpstrFile : null;
    }

    public static string? ShowSaveFileDialog(string filter = "All Files\0*.*\0\0", string initialDir = "")
    {
        OpenFileName ofn = new()
        {
            lpstrFile = new string(new char[256]),
            lpstrFileTitle = new string(new char[64]),
            lpstrFilter = filter,
            lpstrInitialDir = initialDir,
            nFilterIndex = 1,
            Flags = 0x00000002 | 0x00080000
        };

        return GetSaveFileName(ofn) ? ofn.lpstrFile : null;
    }
}
