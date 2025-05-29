namespace TailsManager.Services;

public class TailsManagerService : ILoadableSingleton, IUnloadableSingleton
{
    const string MetadataUrl = "https://datvm.github.io/TimberbornTails/metadata.json";
    const string ImagesUrl = "https://datvm.github.io/TimberbornTails/Tails/{0}";

    static readonly string WorkingFolder = Path.Combine(UserDataFolder.Folder, nameof(TailsManager));

    static readonly string TailsFolder = Path.Combine(WorkingFolder, "Images");
    static readonly string MetaDataPath = Path.Combine(WorkingFolder, "metadata.json");
    static readonly string SubscriptionPath = Path.Combine(WorkingFolder, "subscription.json");

#nullable disable
    TailsMetadata metadata;
    TailsSubscription subscriptions;
    
    public FrozenDictionary<int, TailCollection> CollectionsByIds { get; private set; }
    public FrozenDictionary<int, TailInfo> TailsByIds { get; private set; }
#nullable enable

    public IReadOnlyCollection<int> SubscribedCollectionsIds => subscriptions.CollectionIds;    
    public IReadOnlyCollection<int> SubscribedTailsIds => subscriptions.TailIds;
    public IEnumerable<int> AllSubscribedTailsIds => subscriptions.CollectionIds
        .SelectMany(id => CollectionsByIds[id].Tails.Select(tail => tail.Id))
        .Concat(subscriptions.TailIds)
        .Distinct();
    public IEnumerable<TailInfo> AllSubscribedTails => AllSubscribedTailsIds
        .Select(id => TailsByIds[id]);

    public bool IgnoreFaction => subscriptions.IgnoreFactions;

    readonly HttpClient http = new();

    public void Load()
    {
        Directory.CreateDirectory(TailsFolder);
        LoadMetadata();
        LoadSubscriptions();
    }

    public void Unload()
    {
        http.Dispose();
    }

    public async Task<bool> FetchMetaDataAsync()
    {
        try
        {
            var response = await http.GetStringAsync(MetadataUrl);
            File.WriteAllText(MetaDataPath, response);

            LoadMetadata();

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            return false;
        }
    }

    public void ChangeSubscription(int id, bool isCollection, bool subscribe)
    {
        var collection = isCollection
            ? subscriptions.CollectionIds
            : subscriptions.TailIds;

        if (subscribe)
        {
            collection.Add(id);
        }
        else
        {
            collection.Remove(id);
        }

        SaveSubscriptions();
    }

    public async Task SyncTailImages()
    {
        var subscribedTailsIds = AllSubscribedTailsIds;

        var tailFileNames = subscribedTailsIds
            .Select(q => TailsByIds[q].FileName)
            .ToHashSet();

        var existingFileNames = Directory.GetFiles(TailsFolder, "*.png")
            .Select(Path.GetFileName)
            .ToHashSet();

        foreach (var name in existingFileNames)
        {
            if (tailFileNames.Contains(name))
            {
                tailFileNames.Remove(name);
            }
            else
            {
                File.Delete(Path.Combine(TailsFolder, name));
            }
        }

        foreach (var name in tailFileNames)
        {
            await using var stream = await GetImageAsync(name);

            var path = Path.Combine(TailsFolder, name);
            await using var fs = File.Create(path);
            await stream.CopyToAsync(fs);
        }
    }

    public async Task<Stream> GetImageAsync(string fileName) 
        => await http.GetStreamAsync(string.Format(ImagesUrl, fileName));

    void LoadMetadata()
    {
        metadata = LoadOrDefault(MetaDataPath, () => new TailsMetadata());

        Dictionary<int, TailCollection> collections = [];
        Dictionary<int, TailInfo> tails = [];

        foreach (var col in metadata.Collections)
        {
            collections.Add(col.Id, col);

            foreach (var tail in col.Tails)
            {
                tails.Add(tail.Id, tail);
            }
        }

        CollectionsByIds = collections.ToFrozenDictionary();
        TailsByIds = tails.ToFrozenDictionary();
    }

    void LoadSubscriptions()
    {
        subscriptions = LoadOrDefault(SubscriptionPath, () => new TailsSubscription());
    }

    void SaveSubscriptions()
    {
        Save(SubscriptionPath, subscriptions);
    }

    T LoadOrDefault<T>(string path, Func<T> defaultValue)
    {
        if (!File.Exists(path)) { return defaultValue(); }

        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<T>(json) ?? defaultValue();
    }

    void Save<T>(string path, T data)
    {
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(path, json);
    }

}
