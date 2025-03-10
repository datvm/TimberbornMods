using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Text.Json;

const string Input = """
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1847895984511289}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 1566945626, guid: b5f268a4cb31a93ed707a95d3a502b0e, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  _prefabName: BadwaterSource
  _backwardCompatiblePrefabNames:
  - PollutionSource
  - ToxicantSource
  _requiredFeatureToggle:
  _disablingFeatureToggle:
--- !u!114 &114351026167440829
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1847895984511289}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 1863551748, guid: d8bd5d755ba1f3f1fd82fac824153b08, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  _coordinates:
  - {x: 0, y: 0}
  - {x: 0, y: 1}
  - {x: 0, y: 2}
  - {x: 1, y: 0}
  - {x: 1, y: 1}
  - {x: 1, y: 2}
  - {x: 2, y: 0}
  - {x: 2, y: 1}
  - {x: 2, y: 2}
  _defaultStrength: 3
--- !u!114 &114727693225075664
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1847895984511289}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 447443988, guid: c1ff95bb5ae0e8b420075ec58add6f36, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  _blocksSpec:
    _size: {x: 3, y: 3, z: 2}
    _blockSpecs:
    - _matterBelow: 0
      _legacyOccupation: 0
      _useNewOccupation: 1
      _occupations: 0
      _stackable: 0
      _occupyAllBelow: 0
      _optionallyUnderground: 0
      _underground: 1
    - _matterBelow: 0
      _legacyOccupation: 0
      _useNewOccupation: 1
      _occupations: 0
      _stackable: 0
      _occupyAllBelow: 0
      _optionallyUnderground: 0
      _underground: 1
    - _matterBelow: 0
      _legacyOccupation: 0
      _useNewOccupation: 1
      _occupations: 0
      _stackable: 0
      _occupyAllBelow: 0
      _optionallyUnderground: 0
      _underground: 1
    - _matterBelow: 0
      _legacyOccupation: 0
      _useNewOccupation: 1
      _occupations: 0
      _stackable: 0
      _occupyAllBelow: 0
      _optionallyUnderground: 0
      _underground: 1
    - _matterBelow: 0
      _legacyOccupation: 0
      _useNewOccupation: 1
      _occupations: 0
      _stackable: 0
      _occupyAllBelow: 0
      _optionallyUnderground: 0
      _underground: 1
    - _matterBelow: 0
      _legacyOccupation: 0
      _useNewOccupation: 1
      _occupations: 0
      _stackable: 0
      _occupyAllBelow: 0
      _optionallyUnderground: 0
      _underground: 1
    - _matterBelow: 0
      _legacyOccupation: 0
      _useNewOccupation: 1
      _occupations: 0
      _stackable: 0
      _occupyAllBelow: 0
      _optionallyUnderground: 0
      _underground: 1
    - _matterBelow: 0
      _legacyOccupation: 0
      _useNewOccupation: 1
      _occupations: 0
      _stackable: 0
      _occupyAllBelow: 0
      _optionallyUnderground: 0
      _underground: 1
    - _matterBelow: 0
      _legacyOccupation: 0
      _useNewOccupation: 1
      _occupations: 0
      _stackable: 0
      _occupyAllBelow: 0
      _optionallyUnderground: 0
      _underground: 1
    - _matterBelow: 0
      _legacyOccupation: 0
      _useNewOccupation: 1
      _occupations: 59
      _stackable: 0
      _occupyAllBelow: 0
      _optionallyUnderground: 0
      _underground: 0
    - _matterBelow: 0
      _legacyOccupation: 0
      _useNewOccupation: 1
      _occupations: 59
      _stackable: 0
      _occupyAllBelow: 0
      _optionallyUnderground: 0
      _underground: 0
    - _matterBelow: 0
      _legacyOccupation: 0
      _useNewOccupation: 1
      _occupations: 59
      _stackable: 0
      _occupyAllBelow: 0
      _optionallyUnderground: 0
      _underground: 0
    - _matterBelow: 0
      _legacyOccupation: 0
      _useNewOccupation: 1
      _occupations: 59
      _stackable: 0
      _occupyAllBelow: 0
      _optionallyUnderground: 0
      _underground: 0
    - _matterBelow: 0
      _legacyOccupation: 0
      _useNewOccupation: 1
      _occupations: 59
      _stackable: 0
      _occupyAllBelow: 0
      _optionallyUnderground: 0
      _underground: 0
    - _matterBelow: 0
      _legacyOccupation: 0
      _useNewOccupation: 1
      _occupations: 59
      _stackable: 0
      _occupyAllBelow: 0
      _optionallyUnderground: 0
      _underground: 0
    - _matterBelow: 0
      _legacyOccupation: 0
      _useNewOccupation: 1
      _occupations: 59
      _stackable: 0
      _occupyAllBelow: 0
      _optionallyUnderground: 0
      _underground: 0
    - _matterBelow: 0
      _legacyOccupation: 0
      _useNewOccupation: 1
      _occupations: 59
      _stackable: 0
      _occupyAllBelow: 0
      _optionallyUnderground: 0
      _underground: 0
    - _matterBelow: 0
      _legacyOccupation: 0
      _useNewOccupation: 1
      _occupations: 59
      _stackable: 0
      _occupyAllBelow: 0
      _optionallyUnderground: 0
      _underground: 0
  _entrance:
    _hasEntrance: 0
    _coordinates: {x: 0, y: 0, z: 0}
  _baseZ: 1
  _overridable: 0
  _flippable: 0
--- !u!114 &114197913012095661
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1847895984511289}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: -1706396145, guid: c1ff95bb5ae0e8b420075ec58add6f36, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  _toolGroupId: MapEditor
  _toolOrder: 401
  _toolShape: 0
  _layout: 0
  _customPivot:
    _hasCustomPivot: 0
    _coordinates: {x: 0, y: 0, z: 0}
  _devModeTool: 1
--- !u!114 &114903836202195048
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1847895984511289}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 652365310, guid: b6b9d4cc3090b6fceb2eb6438dd271f1, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  _displayNameLocKey: Building.BadwaterSource.DisplayName
  _descriptionLocKey:
  _flavorDescriptionLocKey: Building.BadwaterSource.FlavorDescription
  _imagePath: Buildings/MapEditor/BadwaterSource/BadwaterSourceIcon
--- !u!114 &114832690767507941
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1847895984511289}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: -787312812, guid: 78ddf52fe1e2cc861f366392e5ec0d99, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  _soundName: BadwaterSource
  _alternativeSoundName:
--- !u!114 &114415431779359851
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1847895984511289}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 1124210722, guid: c1ff95bb5ae0e8b420075ec58add6f36, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  _cutoutTiles:
  - {x: 1, y: 1, z: 1}
--- !u!114 &114749757933741385
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1847895984511289}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 236971078, guid: d8bd5d755ba1f3f1fd82fac824153b08, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  _defaultContamination: 1
--- !u!114 &114490506082544983
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1847895984511289}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 1851264031, guid: 94349ecc19233bb00baa121d76d23cdc, type: 3}
  m_Name:
  m_EditorClassIdentifier:
--- !u!114 &114871204689929284
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1847895984511289}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: -1105938071, guid: c5e228343eebf57a4e1eacc37015c9df, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  _targetNames:
  - '#BadwaterSource'
--- !u!114 &114632011230947345
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1847895984511289}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 1952503208, guid: 79523a2b71f4fae881a60ad2e723d456, type: 3}
  m_Name:
  m_EditorClassIdentifier:
--- !u!114 &114279369307592499
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1847895984511289}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: -1563102196, guid: f52221e1b73d7a1af6ce63101ea50e01, type: 3}
  m_Name:
  m_EditorClassIdentifier:
--- !u!114 &114963846794898679
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1847895984511289}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: -1359597007, guid: 6be17ae7b8f45f8c6eda0436d37598b7, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  _fullModelName: '#Model'
  _uncoveredModelName:
  _undergroundModelName: '#Underground'
  _undergroundModelDepth: 2
--- !u!4 &4981134388116558
Transform:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1921072720561513}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 4736295453232796}
  m_LocalEulerAnglesHint: {x: 0, y: -0, z: -0}
--- !u!65 &65420564594183812
BoxCollider:
  serializedVersion: 3
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1921072720561513}
  m_Material: {fileID: 0}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_LayerOverridePriority: 0
  m_IsTrigger: 0
  m_ProvidesContacts: 0
  m_Enabled: 1
  m_Size: {x: 2.814118, y: 0.12015508, z: 2.811996}
  m_Center: {x: 1.5, y: 0.029931433, z: 1.5}
--- !u!114 &114274258525676496
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1921072720561513}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: -1942782917, guid: 40833514e55bcf2135fbcf9411078554, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  _modelName: Buildings\MapEditor\BadwaterSource\BadwaterSource.Model
--- !u!4 &4739359017447635
Transform:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1838574559098670}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 4736295453232796}
  m_LocalEulerAnglesHint: {x: 0, y: -0, z: -0}
--- !u!114 &114838925006564224
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1838574559098670}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: -1942782917, guid: 40833514e55bcf2135fbcf9411078554, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  _modelName: Buildings/MapEditor/BadwaterSource/BadwaterSource.Underground

""";

FileMapper mapper = new();
await mapper.LoadAsync();

List<GameFile?> output = [];
foreach (var line in Input.Split('\n'))
{
    var guid = ParseForGuid(line);
    if (guid is null) { continue; }

    if (mapper.FilesByGuid.TryGetValue(guid, out var file))
    {
        output.Add(file);
    }
    else
    {
        output.Add(null);
    }
}

Console.WriteLine(string.Join(Environment.NewLine,
    output
        .Select(q => $"\"{q?.Path[FileMapper.AssetFolder.Length..] ?? "N/A"}\",")
        .Distinct()
));

string? ParseForGuid(string line)
{
    if (!line.Contains(' ')) { return line; }

    var start = line.IndexOf("guid: ");
    if (start > -1)
    {
        var actualStart = start + "guid: ".Length;
        var end = line.IndexOf(',', actualStart + 1);

        var guid = line[actualStart..end];
        Console.WriteLine($"Found {guid} from {line}");
        return guid;
    }

    return null;
}

public class FileMapper
{
    public const string AssetFolder = @"D:/Personal/Mods/Timberborn/U7Data/ExportedProject/Assets/";
    const string FileName = "map.json";

    public FrozenDictionary<string, GameFile> FilesByGuid { get; set; } = null!;
    public FrozenDictionary<string, GameFile> FilesByPath { get; set; } = null!;

    public async Task LoadAsync()
    {
        var files = await GetGameFileAsync();

        FilesByGuid = files.ToFrozenDictionary(q => q.Guid);
        FilesByPath = files.ToFrozenDictionary(q => q.Path);
    }

    static async Task<IEnumerable<GameFile>> GetGameFileAsync()
    {
        if (File.Exists(FileName))
        {
            await using var f = File.OpenRead(FileName);
            return await JsonSerializer.DeserializeAsync<IEnumerable<GameFile>>(f)
                ?? throw new InvalidOperationException("Failed to deserialize file.");
        }

        Console.WriteLine("Mapping files...");
        ConcurrentBag<GameFile> files = [];

        await Parallel.ForEachAsync(
            Directory.EnumerateFiles(AssetFolder, "*.meta", SearchOption.AllDirectories), 
            new ParallelOptions()
            {
                MaxDegreeOfParallelism = 1,
            },
            async (path, ct) =>
            {
                await using var f = File.OpenRead(path);
                using var reader = new StreamReader(f);

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync(ct);
                    if (line is null) { return; }

                    if (line.StartsWith("guid"))
                    {
                        var guid = line.Split(' ')[1];

                        path = path[..^".meta".Length].Replace('\\', '/');
                        files.Add(new(guid,
                            path, 
                            Path.GetFileName(path)));
                        return;
                    }
                }                
            });

        await using var fs = File.Create(FileName);
        await JsonSerializer.SerializeAsync(fs, files);

        return files;
    }

}

public readonly record struct GameFile(string Guid, string Path, string Name);