namespace BuildingDecal.Services;

public readonly record struct SpriteWithName(string Name, Sprite Sprite);

public class DecalPictureService(
    IAssetLoader loader,
    IExplorerOpener opener
) : ILoadableSingleton
{
    public static readonly string DecalPath = Path.Combine(UserDataFolder.Folder, "BuildingDecal");
    public const int PixelPerUnit = 128;
    public const string ErrorIconName = "!Unknown";

    public event Action? OnDecalsReloaded;

    public FrozenDictionary<string, SpriteWithName> Decals { get; private set; } = FrozenDictionary<string, SpriteWithName>.Empty;
    public SpriteWithName ErrorIcon { get; private set; }

    static DecalPictureService()
    {
        Directory.CreateDirectory(DecalPath);
    }

    public SpriteWithName GetSprite(string name) => Decals.TryGetValue(name, out var sprite) ? sprite : ErrorIcon;

    public void Load()
    {
        ReloadDecals();
        ErrorIcon = new(ErrorIconName, loader.Load<Sprite>("Sprites/BuildingDecal/question"));
    }

    public void ReloadDecals()
    {
        CleanUp();

        Dictionary<string, SpriteWithName> decals = [];
        var files = Directory.GetFiles(DecalPath, "*.png");
        foreach (var file in files)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var sprite = LoadSprite(file);
                decals[fileName] = new(fileName, sprite);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Failed to load decal image from file '{file}'", ex);
            }
        }

        Decals = decals.ToFrozenDictionary();
        OnDecalsReloaded?.Invoke();
    }

    public void OpenFolder() => opener.OpenDirectory(DecalPath);

    void CleanUp()
    {
        foreach (var d in Decals.Values)
        {
            UnityEngine.Object.Destroy(d.Sprite);
        }
        Decals = FrozenDictionary<string, SpriteWithName>.Empty;
    }

    static Sprite LoadSprite(string filePath)
    {
        var texture = LoadTexture(filePath);
        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, PixelPerUnit);

        return sprite;
    }

    static Texture2D LoadTexture(string filePath)
    {
        var bytes = File.ReadAllBytes(filePath);
        var texture = new Texture2D(2, 2);
        texture.LoadImage(bytes);

        return texture;
    }

}
