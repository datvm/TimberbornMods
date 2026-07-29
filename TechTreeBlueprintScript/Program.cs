const string TechTreeProjectPath = @"D:\Personal\Mods\Timberborn\TimberbornMods\TechTree";
const string TechTreeFileRootPath = TechTreeProjectPath + @"\Root\";

var services = await CreateServicesAsync();
await using var scope = services.CreateAsyncScope();
services = scope.ServiceProvider;

var graphBuilder = services.GetRequiredService<GraphBuilderService>();

// Smoke-test: print Folktails wood-related dependency roots / children.
var graph = graphBuilder.BuildGraph("IronTeeth");

var exporter = services.GetRequiredService<BlueprintExportService>();
await exporter.WriteGraphAsync(TechTreeFileRootPath, graph);

static async Task<IServiceProvider> CreateServicesAsync()
{
    var blueprintProvider = await BlueprintProvider.CreateAsync();

    var col = new ServiceCollection();
    col.AddSingleton(blueprintProvider);

    col.AddServiceSharp();

    return col.BuildServiceProvider();
}