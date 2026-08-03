
namespace Timbermesh2Blender.Services;

public class TextureService
{
    readonly BlueprintProvider bp;

    public string ResourcesPath => bp.GameResourcesFolder;
    public AggregatedCollectionBlueprint Materials { get; }

    FrozenDictionary<string, string> MaterialPaths { get; }

    public TextureService(BlueprintProvider bp)
    {
        this.bp = bp;
        Materials = bp.AggregatedCollections[typeof(MaterialCollectionSpec)];

        Dictionary<string, string> materialPaths = [];
        // Aggregate however it is using name or path so Timbermesh can access them.
    }

    public Stream GetMaterial(string name) // name or path?
        => File.OpenRead(MaterialPaths[name]);

}