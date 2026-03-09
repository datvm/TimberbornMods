namespace TimberLive;

partial class App
{

    bool initialized;

    protected override void OnInitialized()
    {
        Storage.Initialize();

        initialized = true;
    }

}
