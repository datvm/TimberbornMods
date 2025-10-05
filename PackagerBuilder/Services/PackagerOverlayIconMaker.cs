namespace PackagerBuilder.Services;
public sealed class PackagerOverlayIconMaker(IAssetLoader assets) : IDisposable
{

    readonly Texture2D overlay = assets.Load<Texture2D>("Resources/UI/Buildings/Storage/PackagerOverlay");
    readonly Texture2D unpackingOverlay = assets.Load<Texture2D>("Resources/UI/Buildings/Storage/UnpackagerOverlay");

    public byte[] OverlayPackaged(Sprite sprite) => Overlay(sprite, overlay);
    public byte[] OverlayUnpacking(Sprite sprite) => Overlay(sprite, unpackingOverlay);

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

    public void Dispose()
    {
        UnityEngine.Object.Destroy(overlay);
        UnityEngine.Object.Destroy(unpackingOverlay);
    }
}
