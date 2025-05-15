namespace QuickStart.Services;

public class QuickMapEditorService(
    MapRepository mapRepository,
    MapEditorSceneLoader mapEditorSceneLoader, 
    MapValidator mapValidator
)
{

    public void LoadLatestMap()
    {
        var latestMap = GetLatestMap();
        if (latestMap is null) { return; }

        OpenMap(latestMap.Value);
    }

    void OpenMap(MapFileReference map)
    {
        mapValidator.ValidateForMapEditor(map, () =>
        {
            mapEditorSceneLoader.LoadMap(map);
        });
    }

    MapFileReference? GetLatestMap()
    {
        mapRepository.CreateUserMapsDirectory();

        var file = Directory.GetFiles(MapRepository.UserMapsDirectory)
            .Where(q => Path.GetExtension(q) == MapRepository.MapExtension)
            .Select(q => new FileInfo(q))
            .OrderByDescending(q => q.LastWriteTime)
            .FirstOrDefault();
        if (file is null) { return null; }

        var mapName = Path.GetFileNameWithoutExtension(file.Name);
        return MapFileReference.FromUserFolder(mapName);
    }

}
