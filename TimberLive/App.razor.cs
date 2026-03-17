namespace TimberLive;

partial class App
{

    bool initialized;

    protected override void OnInitialized()
    {
        Storage.Initialize();

        foreach (var listener in ApiListeners)
        {
            Api.RegisterConnectionListener(listener);
        }

        initialized = true;
    }

}
