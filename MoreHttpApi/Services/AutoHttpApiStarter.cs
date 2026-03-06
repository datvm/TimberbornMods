namespace MoreHttpApi.Services;

[BindSingleton]
public class AutoHttpApiStarter(HttpApi api) : IPostLoadableSingleton
{
    
    public void PostLoad()
    {
        api.Start();
    }

}
