var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddServices()
    .AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
    .AddSingleton(sp => (IJSInProcessRuntime)sp.GetRequiredService<IJSRuntime>())
    
    .AddSingletonApiListener<Loc>()
    .AddSingletonApiListener<CommonDataService>()
    .AddSingletonApiListener<CommonLiveDataService>()
;

foreach (var t in Assembly.GetExecutingAssembly().GetTypes())
{
    if (t.IsClass && !t.IsAbstract && typeof(IClientAutomationBuilding).IsAssignableFrom(t))
    {
        builder.Services.AddSingleton(typeof(IClientAutomationBuilding), t);
    }
}

await builder.Build().RunAsync();
