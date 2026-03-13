namespace MoreHttpApi.Services;

[BindSingleton]
public class AutoHttpApiStarter(HttpApi api, MSettings s) : IPostLoadableSingleton
{
    
    public void PostLoad()
    {
        if (!s.AutoStartApi.Value) { return; }

        api.SetPort((ushort) s.AutoStartPort.Value);
        api.Start();
    }

}
