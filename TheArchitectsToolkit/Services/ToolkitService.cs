
namespace TheArchitectsToolkit.Services;

public class ToolkitService(MSettings s) : ILoadableSingleton
{
    static readonly FieldInfo maxMapSizeField = typeof(NewMapBox).GetField(nameof(NewMapBox.MaxMapSize), BindingFlags.NonPublic | BindingFlags.Static);
    
    static int? originalMaxMapSizeValue;

    public void Load()
    {
        s.OnSettingsChanged += OnSettingsChanged;
        OnSettingsChanged();
    }

    private void OnSettingsChanged()
    {
        SetMapSize();
    }

    void SetMapSize()
    {
        originalMaxMapSizeValue ??= (int)maxMapSizeField.GetValue(null);

        maxMapSizeField.SetValue(null, MSettings.UnlimitedMapSize ? int.MaxValue : originalMaxMapSizeValue.Value);
    }

}
