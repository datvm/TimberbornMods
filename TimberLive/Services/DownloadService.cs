namespace TimberLive.Services;

[SelfService(Lifetime = ServiceLifetime.Singleton)]
public class DownloadService(IJSRuntime js)
{

    public async Task DownloadFileAsync(Stream stream, string fileName)
    {
        stream.Seek(0, SeekOrigin.Begin);

        using var streamRef = new DotNetStreamReference(stream, true);
        await js.InvokeVoidAsync("BlazorHelper.saveFileAsync", streamRef, fileName);
    }

}
