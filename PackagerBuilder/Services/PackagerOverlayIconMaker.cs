namespace PackagerBuilder.Services;
public sealed class PackagerOverlayIconMaker(IAssetLoader assets) : ILoadableSingleton
{

#nullable disable 
    Texture2D overlay, unpackingOverlay, crateOverlay, crateUnpackOverlay, liquidBarrelOverlay, liquidBarrelUnpackOverlay;
#nullable enable
    string? basePath;

    public void Load()
    {
        overlay = assets.Load<Texture2D>("Resources/UI/Buildings/Storage/PackagerOverlay");
        unpackingOverlay = assets.Load<Texture2D>("Resources/UI/Buildings/Storage/UnpackagerOverlay");

        crateOverlay = assets.Load<Texture2D>("Resources/UI/Buildings/Storage/BulkCrateOverlay");
        crateUnpackOverlay = assets.Load<Texture2D>("Resources/UI/Buildings/Storage/BulkCrateUnpackOverlay");

        liquidBarrelOverlay = assets.Load<Texture2D>("Resources/UI/Buildings/Storage/LiquidBarrelOverlay");
        liquidBarrelUnpackOverlay = assets.Load<Texture2D>("Resources/UI/Buildings/Storage/LiquidBarrelUnpackOverlay");
    }

    public void SetExportPath(string? path) => basePath = path;

    public void MakeExportFolder(string path)
    {
        ThrowIfNoBasePath();

        var fullPath = Path.Combine(basePath, path);
        Directory.CreateDirectory(fullPath);
    }

    public void OverlayTo(Sprite sprite, OverlayType type, string path)
    {
        ThrowIfNoBasePath();

        var overlayedBytes = Overlay(sprite, type);
        var fullPath = Path.Combine(basePath, path);
        File.WriteAllBytes(fullPath, overlayedBytes);
    }

    public byte[] Overlay(Sprite sprite, OverlayType type) => Overlay(sprite, type switch
    {
        OverlayType.Packaged => overlay,
        OverlayType.Unpacking => unpackingOverlay,
        OverlayType.CratePackaged => crateOverlay,
        OverlayType.CrateUnpacking => crateUnpackOverlay,
        OverlayType.LiquidBarrelPackaged => liquidBarrelOverlay,
        OverlayType.LiquidBarrelUnpacking => liquidBarrelUnpackOverlay,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    });

    void ThrowIfNoBasePath()
    {
        if (basePath is null)
        {
            throw new InvalidOperationException("Base path is not set. Call SetExportPath first.");
        }
    }

    byte[] Overlay(Sprite sprite, Texture2D overlay)
    {
        var texture = sprite.texture;
        var overlayed = OverlayBottomRight(texture, overlay);
        return overlayed.EncodeToPNG();
    }

    public static Texture2D OverlayBottomRight(Texture2D baseTexture, Texture2D overlayTexture, int padding = 0)
    {
        var width = baseTexture.width;
        var height = baseTexture.height;
        Texture2D result = new(width, height, baseTexture.format, false);
        result.SetPixels(baseTexture.GetPixels());

        var posX = width - overlayTexture.width - padding;
        var posY = padding; // bottom right corner + padding

        Color[] overlayPixels = overlayTexture.GetPixels();
        var ow = overlayTexture.width;
        var oh = overlayTexture.height;

        for (var y = 0; y < oh; y++)
        {
            for (var x = 0; x < ow; x++)
            {
                var px = posX + x;
                var py = posY + y;
                if (px < 0 || px >= width || py < 0 || py >= height) continue;

                Color baseColor = result.GetPixel(px, py);
                Color overlayColor = overlayPixels[x + y * ow];
                Color final = Color.Lerp(baseColor, overlayColor, overlayColor.a);
                result.SetPixel(px, py, final);
            }
        }

        result.Apply();
        return result;
    }

    public enum OverlayType
    {
        Packaged,
        Unpacking,
        CratePackaged,
        CrateUnpacking,
        LiquidBarrelPackaged,
        LiquidBarrelUnpacking
    }

}
