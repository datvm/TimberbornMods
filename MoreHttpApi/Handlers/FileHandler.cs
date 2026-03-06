namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class FileHandler(IAssetLoader assets) : IMoreHttpApiHandler
{
    public string Endpoint { get; } = "file";

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
    {
        var remaining = parsedRequestPath.RemainingSegment;
        if (remaining.Length < 1) { return false; }

        var type = remaining[0];
        var path = parsedRequestPath.QueryParameters.Get("path");
        if (string.IsNullOrEmpty(path)) { return false; }


        switch (type)
        {
            case "image":
                var bytes = await GetImageAsync(path);
                await context.Write("image/png", bytes);

                return true;
            default:
                return false;
        }
    }

    async Task<byte[]> GetImageAsync(string path)
    {
        await Awaitable.MainThreadAsync();

        var texture = assets.Load<Texture2D>(path);
        texture = DeCompress(texture);
        return texture.EncodeToPNG();
    }

    // From:https://stackoverflow.com/questions/51315918/how-to-encodetopng-compressed-textures-in-unity
    static Texture2D DeCompress(Texture source)
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
