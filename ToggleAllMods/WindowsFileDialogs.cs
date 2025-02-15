using System.Runtime.InteropServices;

namespace ToggleAllMods;

public class WindowsFileDialogs
{
    [DllImport("comdlg32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

    [DllImport("comdlg32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetSaveFileName([In, Out] OpenFileName ofn);

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
        OpenFileName ofn = new OpenFileName();
        ofn.lpstrFile = new string(new char[256]);
        ofn.lpstrFileTitle = new string(new char[64]);
        ofn.lpstrFilter = filter;
        ofn.lpstrInitialDir = initialDir;
        ofn.nFilterIndex = 1;
        ofn.Flags = 0x00000008 | 0x00080000;

        if (GetOpenFileName(ofn))
        {
            return ofn.lpstrFile;
        }
        return null;
    }

    public static string? ShowSaveFileDialog(string filter = "All Files\0*.*\0\0", string initialDir = "")
    {
        OpenFileName ofn = new OpenFileName();
        ofn.lpstrFile = new string(new char[256]);
        ofn.lpstrFileTitle = new string(new char[64]);
        ofn.lpstrFilter = filter;
        ofn.lpstrInitialDir = initialDir;
        ofn.nFilterIndex = 1;
        ofn.Flags = 0x00000002 | 0x00080000;

        if (GetSaveFileName(ofn))
        {
            return ofn.lpstrFile;
        }
        return null;
    }
}
