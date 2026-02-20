namespace BuildingBlueprints.Services;

[BindSingleton]
public class BlueprintThumbnailPicker(ISystemFileDialogService diag)
{

    public string? Pick() => diag.ShowOpenFileDialog(".png;.jpg;.jpeg");

    public Texture2D? LoadThumbnail(string? path)
    {
        if (path is null || !File.Exists(path)) { return null; }

        try
        {
            var fileData = File.ReadAllBytes(path);
            var t = new Texture2D(2, 2);
            return t.LoadImage(fileData) ? t : null;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Failed to load thumbnail: " + ex);
            return null;
        }
    }

}
