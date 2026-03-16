namespace TimberLive.Helpers;

public static class DIExtensions
{

    extension(IServiceCollection services)
    {
        public IServiceCollection AddSingletonApiListener<T>()
            where T : class, IApiConnectionListener 
            => services
                .AddSingleton<T>()
                .AddSingleton<IApiConnectionListener>(sp => sp.GetRequiredService<T>());
    }

}
