namespace TimberUi.Services;

[BindSingleton(Contexts = BindAttributeContext.Bootstrapper, Exported = true)]
public class TextTextureFontService
{
    public static readonly string[] MonospaceFonts =
    [
        "Cascadia Mono", "Consolas", "Courier New", // Windows
        "SF Mono", "Menlo", "Monaco", "Courier", // Mac
        "DejaVu Sans Mono", "Liberation Mono", "Ubuntu Mono", "Noto Sans Mono", // Linux
    ];

    public static readonly string[] DefaultFonts =
    [
        "Segoe UI", "Arial", "Calibri", "Tahoma", // Windows
        "SF Pro", "Helvetica Neue", "Helvetica", "Arial", // Mac
        "Noto Sans", "DejaVu Sans", "Liberation Sans", "Ubuntu", // Linux
    ];

    public FrozenSet<string> InstalledFontSet { get; private set; } = [];
    public ImmutableArray<string> InstalledFonts { get; private set; } = [];
    public bool Initialized => InstalledFonts.Length > 0;

    public string? MonospaceFont { get; private set; }
    public string? DefaultFont { get; private set; }

    public void EnsureInitialized()
    {
        if (!Initialized)
        {
            ReloadInstalledFonts();
        }
    }

    public void ReloadInstalledFonts()
    {
        var fontNames = Font.GetOSInstalledFontNames();
        
        InstalledFontSet = [..fontNames];
        InstalledFonts = [.. fontNames];

        MonospaceFont = ResolveFont(MonospaceFonts);
        DefaultFont = ResolveFont(DefaultFonts);
    }

    public string? ResolveFont(string[] preferredFonts)
    {
        EnsureInitialized();

        foreach (var font in preferredFonts)
        {
            if (InstalledFontSet.Contains(font))
            {
                return font;
            }
        }
        return null;
    }

    public void PopulateDropdown(DropdownRow<string> dropdown)
    {
        EnsureInitialized();
        dropdown.SetItems(InstalledFonts, f => f, true);
    }

}
