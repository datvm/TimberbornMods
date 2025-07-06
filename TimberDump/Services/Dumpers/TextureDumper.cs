namespace TimberDump.Services.Dumpers;

public class TextureDumper(
    IAssetLoader assets,
    IExplorerOpener opener
)
{
    public static string DumpFolder = Path.Combine(DumpService.DumpFolder, "Images");
    

    public void Dump()
    {
        Directory.CreateDirectory(DumpFolder);

        var textures = assets.LoadAll<Texture2D>("");

        foreach (var asset in textures)
        {
            var texture = asset.Asset;
            if (!texture || texture.width == 0 || texture.height == 0) { continue; }

            var fileName = $"{texture.name}.png";
            var filePath = Path.Combine(DumpFolder, fileName);
            
            Debug.Log($"  Dumping {fileName}");
            var decompressed = DeCompress(texture);
            File.WriteAllBytes(filePath, decompressed.EncodeToPNG());
        }

        opener.OpenDirectory(DumpFolder);
    }

    // From:https://stackoverflow.com/questions/51315918/how-to-encodetopng-compressed-textures-in-unity
    public static Texture2D DeCompress(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
}
