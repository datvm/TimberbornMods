
namespace BuffDebuff;

public class BuffService(BuffEntityService buffEntities, IEnumerable<IBuff> buffs, ISingletonLoader loader, EntityService entities, EventBus eventBus)
    : ISaveableSingleton, ILoadableSingleton, IBuffService, ITickableSingleton
{
    static readonly SingletonKey SaveKey = new("BuffService");
    static readonly ListKey<string> BuffIds = new("BuffIds");
    static readonly ListKey<string> ActiveInstanceIds = new("ActiveInstanceIds");

    FrozenDictionary<long, IBuff> allBuffs = null!;

    public IBuff this[long id] => allBuffs[id];
    public IEnumerable<IBuff> Buffs => allBuffs.Values;

    readonly Dictionary<long, BuffInstance> buffInstances = [];
    readonly Dictionary<BuffInstance, List<BuffableComponent>> affected = [];
    public IEnumerable<BuffInstance> ActiveInstances => buffInstances.Values;
    HashSet<long> expectingLoadInstances = [];

    public void Load()
    {
        LoadExistingKeys();

        foreach (var b in buffs)
        {
            Register(b);
        }

        allBuffs = buffs.ToFrozenDictionary(q => q.Id);
    }

    public void OnInstanceLoaded<T>(T instance) where T : BuffInstance
    {
        if (!expectingLoadInstances.Contains(instance.Id))
        {
            Debug.LogWarning($"Unexpected buff instance: {instance.Id}. It will be ignored");
        }

        Apply(instance);
        expectingLoadInstances.Remove(instance.Id);
    }

    public void Apply<T>(T instance) where T : BuffInstance
    {
        if (!allBuffs.ContainsKey(instance.Buff.Id))
        {
            Debug.LogError($"Buff {instance.Buff.Name} ({instance.Buff.Id}) is not registered. Ignoring request.");
            return;
        }

        buffEntities.Register(instance);
        CreateAndAttachBuffInstance(instance);

        foreach (var item in instance.Targets)
        {
            buffEntities.Register(item);
        }

        foreach (var item in instance.Effects)
        {
            buffEntities.Register(item);
        }

        buffInstances[instance.Id] = instance;

        foreach (var item in instance.Targets)
        {
            item.Init();
        }

        foreach (var item in instance.Effects)
        {
            item.Init();
        }
        instance.Init();

        eventBus.Post(new BuffInstanceAppliedEvent(instance));
    }

    public void Remove<T>(T instance) where T : BuffInstance
    {
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
            Debug.LogWarning($"Buff instance {instance.Buff.Name}({instance.Id}) is already {(active ? "active" : "inactive")}. Ignoring request.");
            return;
        }

        instance.Active = active;

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

    EntityComponent CreateAndAttachBuffInstance<T>(T instance) where T : BuffInstance
    {
        return entities.Instantiate(instance);
    }

    void Register(IBuff b)
    {
        buffEntities.Register(b);
    }

    void LoadExistingKeys()
    {
        if (!loader.HasSingleton(SaveKey)) { return; }

        var s = loader.GetSingleton(SaveKey);
        var ids = s.Get(BuffIds)
            .Select(q => q.Split(';'))
            .ToFrozenDictionary(q => q[0], q => q[1]);

        foreach (var buff in buffs)
        {
            if (ids.TryGetValue(buff.GetType().FullName, out var rawId))
            {
                buff.Id = long.Parse(rawId);
            }
        }

        expectingLoadInstances = s.Get(ActiveInstanceIds)
            .Select(long.Parse)
            .ToHashSet();
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);

        s.Set(BuffIds, buffs
            .Select(q => $"{q.GetType().FullName};{q.Id}")
            .ToImmutableArray());
        s.Set(ActiveInstanceIds, buffInstances.Keys
            .Select(q => q.ToString())
            .ToImmutableArray());
    }

    public void Tick()
    {
        if (expectingLoadInstances.Count > 0)
        {
            Debug.LogWarning("Some buff instances were not restored. It may be intentional.");
            expectingLoadInstances.Clear();
        } 

        ProcessBuffs();
    }

    void ProcessBuffs()
    {
        foreach (var b in ActiveInstances)
        {
            ProcessBuff(b);
        }
    }

    void ProcessBuff(BuffInstance b)
    {
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

        if (!targetsChanged) { return; }

        HashSet<BuffableComponent> buffables = [];
        foreach (var target in b.Targets)
        {
            buffables.AddRange(target.Targets);
        }

        ApplyBuffTo(b, buffables);

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
