namespace Packager.Services;

public class PackagerOverlayIconMaker(IAssetLoader assets)
{

    Texture2D? overlay;

    public Sprite Overlay(Sprite sprite)
    {
        overlay ??= assets.Load<Texture2D>("Resources/UI/Buildings/Storage/PackagerOverlay");

        var texture = sprite.texture;
        var overlayed = OverlayBottomRight(texture, overlay);

        return Sprite.Create(overlayed, sprite.rect, sprite.pivot);
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



}
