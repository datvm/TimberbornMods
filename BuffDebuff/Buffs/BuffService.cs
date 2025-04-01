namespace BuffDebuff;

public class BuffService(
    IBuffEntityService buffEntities,
    ISingletonLoader loader,
    IContainer container,
    EventBus eventBus
) : ISaveableSingleton, ILoadableSingleton, IPostLoadableSingleton, IBuffService, ITickableSingleton
{
    static readonly SingletonKey SaveKey = new("BuffService");
    static readonly ListKey<string> BuffIdsKey = new("BuffIds");
    static readonly PropertyKey<string> ActiveInstancesKey = new("ActiveInstance");

    readonly Dictionary<long, IBuff> allBuffs = [];

    public IBuff this[long id] => allBuffs[id];
    public IEnumerable<IBuff> Buffs => allBuffs.Values;

    readonly Dictionary<long, BuffInstance> buffInstances = [];
    readonly Dictionary<BuffInstance, List<BuffableComponent>> affected = [];
    public IEnumerable<BuffInstance> ActiveInstances => buffInstances.Values;
    Dictionary<string, long> expectingLoadBuffs = [];

    public void Load()
    {
        LoadExistingKeys();
    }

    public void Apply<T>(T instance) where T : BuffInstance
    {
        if (!allBuffs.ContainsKey(instance.Buff.Id))
        {
            Debug.LogError($"Instace {instance.Id} of {instance.Buff.GetHumanFriendlyId()} is not registered. Ignoring request.");
            return;
        }

        buffEntities.Register(instance);

        foreach (var item in instance.Targets)
        {
            buffEntities.Register(item);
        }

        foreach (var item in instance.Effects)
        {
            buffEntities.Register(item);
        }

        buffInstances[instance.Id] = instance;
        instance.Init();

        foreach (var item in instance.Targets)
        {
            item.Init();
        }

        foreach (var item in instance.Effects)
        {
            item.Init();
        }

        eventBus.Post(new BuffInstanceAppliedEvent(instance));
    }

    public void Remove<T>(T instance) where T : BuffInstance
    {
        var affectedBuffables = affected.TryGetValue(instance, out var list) ? list : [];
        foreach (var buffable in affectedBuffables)
        {
            buffable.RemoveBuff(instance);
        }

        foreach (var item in instance.Targets)
        {
            item.CleanUp();
        }
        foreach (var item in instance.Effects)
        {
            item.CleanUp();
        }
        instance.CleanUp();

        buffEntities.Unregister(instance);
        foreach (var item in instance.Targets)
        {
            buffEntities.Unregister(item);
        }
        foreach (var item in instance.Effects)
        {
            buffEntities.Unregister(item);
        }

        buffInstances.Remove(instance.Id);
        affected.Remove(instance);
    }

    public void SetActive<T>(T instance, bool active) where T : BuffInstance
    {
        if (instance.Active == active)
        {
            Debug.LogWarning($"Buff instance {instance.Id} from {instance.Buff.GetHumanFriendlyId()} is already {(active ? "active" : "inactive")}." +
                $" Ignoring request.");
            return;
        }

        instance.Active = active;

        var buffables = affected.TryGetValue(instance, out var list) ? list : [];
        foreach (var buffable in buffables)
        {
            buffable.BuffActiveChanged(instance);
        }

        if (active)
        {
            eventBus.Post(new BuffInstanceActivatedEvent(instance));
        }
        else
        {
            eventBus.Post(new BuffInstanceDeactivatedEvent(instance));
        }
    }

    public void Activate<T>(T instance) where T : BuffInstance
    {
        SetActive(instance, true);
    }

    public void Deactivate<T>(T instance) where T : BuffInstance
    {
        SetActive(instance, false);
    }

    public TInstance CreateBuffInstance<TBuff, TInstance>(TBuff buff)
        where TBuff : IBuff
        where TInstance : BuffInstance, IBuffInstance<TBuff>, new()
    {
        if (!allBuffs.TryGetValue(buff.Id, out var buffType))
        {
            throw new InvalidOperationException($"{buff.GetHumanFriendlyId()} was not registered");
        }

        if ((IBuff)buff != buffType)
        {
            throw new InvalidOperationException(
                $"{buff.GetHumanFriendlyId()} is not the same as the registered buff {buffType.GetHumanFriendlyId()}");
        }

        var instance = new TInstance();
        instance.SetBuff(buff);
        container.Inject(instance);

        return instance;
    }

    public TInstance CreateBuffInstance<TBuff, TInstance, TValue>(TBuff buff, TValue value)
        where TBuff : IBuff
        where TInstance : BuffInstance, IBuffInstance<TBuff>, IValuedBuffInstance<TValue>, new()
        where TValue : notnull
    {
        var comp = CreateBuffInstance<TBuff, TInstance>(buff);
        comp.Value = value;

        return comp;
    }

    public void Register<T>(T b) where T : IBuff
    {
        var t = b.GetType().FullName;
        var expectingType = expectingLoadBuffs.TryGetValue(t, out var expectingId);
        if (b.Id > 0)
        {
            if (!expectingType)
            {
                Debug.LogError($"The save is not expecting the {b.GetHumanFriendlyId()}. Will register as a new Buff (you will gain a new Id)");
                b.Id = 0;
            }

            if (expectingId != b.Id)
            {
                Debug.LogError($"The save is expecting the buff with Type {t} to have ID {expectingId} " +
                    $"but instead received {b.GetHumanFriendlyId()}. Ignoring request.");
                return;
            }
        }
        else if (expectingType)
        {
            Debug.LogError($"The save is expecting the type {t} to have ID {expectingId} but instead is receving one without ID ({b.GetHumanFriendlyId()}). "
                + $"Ignoring request.");
            return;
        }

        buffEntities.Register(b);


        allBuffs.Add(b.Id, b);

        expectingLoadBuffs.Remove(t);
    }

    public IEnumerable<T> GetInstances<T>() where T : BuffInstance
    {
        return [..buffInstances.Values
            .Where(q => q is T)
            .Cast<T>()];
    }

    public void RemoveAllInstances<T>() where T : BuffInstance
    {
        foreach (var instance in GetInstances<T>())
        {
            Remove(instance);
        }
    }

    void LoadExistingKeys()
    {
        if (!loader.HasSingleton(SaveKey)) { return; }

        var s = loader.GetSingleton(SaveKey);

        expectingLoadBuffs = s.Get(BuffIdsKey)
            .Select(q => q.Split(';'))
            .ToDictionary(q => q[0], q => long.Parse(q[1]));
    }

    public void PostLoad()
    {
        if (expectingLoadBuffs.Count > 0)
        {
            foreach (var buff in expectingLoadBuffs)
            {
                Debug.LogWarning($"The save is expecting the type {buff.Key} with ID {buff.Value} to register but it did not. Instances of those buffs will be ignored");
            }

            expectingLoadBuffs.Clear();
        }

        LoadInstances();
    }

    void LoadInstances()
    {
        if (!loader.HasSingleton(SaveKey)) { return; }
        var s = loader.GetSingleton(SaveKey);

        var rawJson = s.Get(ActiveInstancesKey);
        var entries = JsonConvert.DeserializeObject<IEnumerable<BuffInstanceSaveEntry>>(rawJson) ?? [];

        foreach (var entry in entries)
        {
            if (!allBuffs.TryGetValue(entry.BuffId, out var buff))
            {
                Debug.LogError($"Buff with ID {entry.BuffId} was not registered. Ignoring request.");
                continue;
            }

            var type = BuffDebuffUtils.GetTypeFrom(entry.Type);
            if (type == null)
            {
                Debug.LogError($"Type {entry.Type} was not found. Ignoring request.");
                continue;
            }

            var constructor = type.GetConstructor([]);
            if (constructor == null)
            {
                Debug.LogError($"Type {entry.Type} does not have a parameterless constructor. Ignoring request.");
                continue;
            }

            var instance = (BuffInstance)constructor.Invoke([]);
            instance.SetBuff(buff);
            container.Inject(instance);

            if (instance.Load(entry.SavedData))
            {
                Apply(instance);
            }
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);

        s.Set(BuffIdsKey, [.. allBuffs.Values.Select(q => $"{q.GetType().FullName};{q.Id}")]);

        IEnumerable<BuffInstanceSaveEntry> entries = GetSaveEntries();
        s.Set(ActiveInstancesKey, JsonConvert.SerializeObject(entries));
    }

    IEnumerable<BuffInstanceSaveEntry> GetSaveEntries()
    {
        foreach (var entry in buffInstances.Values)
        {
            var save = entry.Save();
            if (save is null) { continue; } // This instance chose not to save

            yield return new BuffInstanceSaveEntry(entry.GetType().FullName, entry.Buff.Id, save);
        }
    }

    public void Tick()
    {
        ProcessBuffs();
    }

    void ProcessBuffs()
    {
        foreach (var b in ActiveInstances.ToList())
        {
            ProcessBuff(b);
        }
    }

    void ProcessBuff(BuffInstance b)
    {
        b.Update();
        if (!b.Active) { return; }

        var targetsChanged = false;
        foreach (var target in b.Targets)
        {
            target.UpdateTargets();
            if (target.TargetsChanged)
            {
                targetsChanged = true;
            }
        }

        if (targetsChanged)
        {
            HashSet<BuffableComponent> buffables = [];
            foreach (var target in b.Targets)
            {
                buffables.AddRange(target.Targets);
            }

            ApplyBuffTo(b, buffables);
        }

        foreach (var e in b.Effects)
        {
            e.UpdateEffect();
        }
    }

    void ApplyBuffTo(BuffInstance instance, HashSet<BuffableComponent> buffables) // Should be a HashSet instead of IEnumerable because of performance
    {
        var existing = affected.TryGetValue(instance, out var list) ? list : (affected[instance] = []);
        var newTargets = buffables.Where(q => !existing.Contains(q));
        var removedTargets = existing.Where(q => !buffables.Contains(q));

        foreach (var b in existing)
        {
            b.RemoveBuff(instance);
        }

        foreach (var b in buffables)
        {
            b.ApplyBuff(instance);
        }
        existing.Clear();
        existing.AddRange(buffables);
    }

}

readonly record struct BuffInstanceSaveEntry(string Type, long BuffId, string SavedData);