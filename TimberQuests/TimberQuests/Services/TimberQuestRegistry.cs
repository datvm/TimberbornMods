namespace TimberQuests;

public class TimberQuestRegistry(
    ISpecService specs,
    ILoc t
) : ILoadableSingleton
{

    FrozenDictionary<string, TimberQuestSpec> allSpecsByIds = null!;

    public TimberQuestSpec Get(string id) => allSpecsByIds[id];
    public TimberQuestSpec? GetOrDefault(string id) => allSpecsByIds.TryGetValue(id, out var spec) ? spec : null;
    public bool TryGet(string id, [MaybeNullWhen(false)] out TimberQuestSpec spec)
        => allSpecsByIds.TryGetValue(id, out spec);
    public bool Has(string id) => allSpecsByIds.ContainsKey(id);

    public void Load()
    {
        LoadSpecs();
    }

    void LoadSpecs()
    {
        IEnumerable<TimberQuestSpec> quests;
        try
        {
            quests = [.. specs.GetSpecs<TimberQuestSpec>()];
        }
        catch (NullReferenceException)
        {
            quests = [];
        }

        Dictionary<string, TimberQuestSpec> dict = [];

        foreach (var q in quests)
        {
            if (dict.ContainsKey(q.Id))
            {
                throw new InvalidDataException($"Quest Id already exist: {q}");
            }

            InitQuest(q);

            dict[q.Id] = q;
        }

        allSpecsByIds = dict.ToFrozenDictionary();
    }

    void InitQuest(TimberQuestSpec q)
    {
        if (string.IsNullOrEmpty(q.Id))
        {
            throw new InvalidDataException($"{q} ({q.NameKey}) has no Id.");
        }

        if (q.Id.Contains(';'))
        {
            throw new InvalidDataException($"{q} cannot contain ';'");
        }

        if (q.Steps.Length == 0 || !q.Steps.FastAny(s => !s.Disabled))
        {
            throw new InvalidDataException($"{q} has no steps or all the steps are disabled.");
        }

        q.Name = q.NameKey.T(t);

        if (q.DescriptionKey is not null)
        {
            q.Description = q.DescriptionKey.TFormat(t, q.Parameters);
        }

        if (q.RewardsKey is not null)
        {
            q.Rewards = q.RewardsKey.TFormat(t, q.Parameters);
        }

        InitQuestSteps(q);
    }

    void InitQuestSteps(TimberQuestSpec q)
    {
        foreach (var s in q.Steps)
        {
            if (s.Disabled) { continue; }

            s.Name = s.NameKey.T(t);

            if (s.DescriptionKey is not null)
            {
                s.Description = s.DescriptionKey.TFormat(t, q.Parameters)
                    .Replace("[[", "__SPECIAL[__")
                    .Replace("]]", "__SPECIAL]__")
                    .Replace("[", "{")
                    .Replace("]", "}")
                    .Replace("__SPECIAL[__", "[[")
                    .Replace("__SPECIAL]__", "]]");
            }
        }
    }

}
