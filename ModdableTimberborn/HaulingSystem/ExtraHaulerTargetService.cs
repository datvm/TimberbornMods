namespace ModdableTimberborn.HaulingSystem;

/// <summary>
/// Registers extra inventories as district-scoped haul destinations so haulers can
/// deliver without finished <see cref="HaulCandidate"/> buildings.
/// Uses the destination's vanilla <see cref="FillInputWorkplaceBehavior"/>.
/// Registration <c>Districts</c> control which hauling posts are offered the job;
/// stock search for registered targets uses the <b>hauler's workplace district</b>
/// (see <see cref="Patches.CarrierInventoryFinderPatches"/>).
/// </summary>
public class ExtraHaulerTargetService : ILoadableSingleton, IUnloadableSingleton
{
    /// <summary>Same as vanilla <c>HaulCandidate.PriorityFactor</c>.</summary>
    public const float PriorityFactor = 0.5f;

    /// <summary>Hot-path access while the game scene is loaded; null outside.</summary>
    public static ExtraHaulerTargetService? Instance { get; private set; }

    readonly Dictionary<Inventory, Entry> byInventory = [];
    readonly Dictionary<DistrictCenter, HashSet<Inventory>> byDistrict = [];

    public void Load()
    {
        Instance = this;
    }

    public void Unload()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        byInventory.Clear();
        byDistrict.Clear();
    }

    public IDisposable AddExtraTarget(ExtraHaulerTargetRegistration registration)
    {
        if (!registration.Inventory)
        {
            throw new ArgumentException("Inventory is required.", nameof(registration));
        }

        if (registration.Districts is null || registration.Districts.Count == 0)
        {
            throw new ArgumentException("At least one district is required.", nameof(registration));
        }

        var fill = registration.Inventory.GetComponent<FillInputWorkplaceBehavior>();
        if (!fill)
        {
            throw new InvalidOperationException(
                $"Inventory '{registration.Inventory.Name}' has no {nameof(FillInputWorkplaceBehavior)}. " +
                $"Add that vanilla component to the entity (e.g. ConstructionSite is decorated when " +
                $"{nameof(ModdableTimberbornRegistry.UseExtraHaulerTargets)} is enabled), then register.");
        }

        RemoveExtraTarget(registration.Inventory);

        var entry = new Entry(registration, fill, Prioritized: false);
        byInventory[registration.Inventory] = entry;

        foreach (var district in registration.Districts)
        {
            if (!district)
            {
                continue;
            }

            if (!byDistrict.TryGetValue(district, out var set))
            {
                set = [];
                byDistrict[district] = set;
            }

            set.Add(registration.Inventory);
        }

        return new RegistrationHandle(this, registration.Inventory);
    }

    public void RemoveExtraTarget(Inventory inventory)
    {
        if (!inventory || !byInventory.TryGetValue(inventory, out var entry))
        {
            return;
        }

        byInventory.Remove(inventory);

        foreach (var district in entry.Registration.Districts)
        {
            if (!district || !byDistrict.TryGetValue(district, out var set))
            {
                continue;
            }

            set.Remove(inventory);
            if (set.Count == 0)
            {
                byDistrict.Remove(district);
            }
        }
    }

    /// <summary>
    /// Mirrors vanilla haul prioritizable boost. No-op if not registered.
    /// </summary>
    public void SetExtraTargetPrioritized(Inventory inventory, bool prioritized)
    {
        if (!inventory || !byInventory.TryGetValue(inventory, out var entry))
        {
            return;
        }

        byInventory[inventory] = entry with { Prioritized = prioritized };
    }

    public bool IsExtraTargetPrioritized(Inventory inventory)
        => inventory
            && byInventory.TryGetValue(inventory, out var entry)
            && entry.Prioritized;

    public bool TryGetRegistration(
        Inventory inventory,
        out ExtraHaulerTargetRegistration registration)
    {
        if (inventory && byInventory.TryGetValue(inventory, out var entry))
        {
            registration = entry.Registration;
            return true;
        }

        registration = default;
        return false;
    }

    public IReadOnlyList<ExtraHaulerTargetRegistration> GetTargetsForDistrict(DistrictCenter district)
    {
        if (!district || !byDistrict.TryGetValue(district, out var set))
        {
            return [];
        }

        List<ExtraHaulerTargetRegistration> result = [];
        List<Inventory>? dead = null;

        foreach (var inventory in set)
        {
            if (!inventory || !byInventory.TryGetValue(inventory, out var entry))
            {
                dead ??= [];
                dead.Add(inventory);
                continue;
            }

            result.Add(entry.Registration);
        }

        if (dead is not null)
        {
            foreach (var inventory in dead)
            {
                RemoveExtraTarget(inventory);
            }
        }

        return result;
    }

    /// <summary>
    /// Appends weighted vanilla fill behaviors for this district (caller sorts).
    /// </summary>
    internal void AppendWeightedBehaviors(
        DistrictCenter district,
        IList<WeightedBehavior> weightedBehaviors)
    {
        if (!district || !byDistrict.TryGetValue(district, out var set))
        {
            return;
        }

        List<Inventory>? dead = null;

        foreach (var inventory in set)
        {
            if (!inventory || !byInventory.TryGetValue(inventory, out var entry))
            {
                dead ??= [];
                dead.Add(inventory);
                continue;
            }

            if (!entry.Registration.Inventory.Enabled || !entry.Fill)
            {
                continue;
            }

            var weight = ApplyPrioritize(entry.Registration.Weight, entry.Prioritized);
            weightedBehaviors.Add(new WeightedBehavior(weight, entry.Fill));
        }

        if (dead is not null)
        {
            foreach (var inventory in dead)
            {
                RemoveExtraTarget(inventory);
            }
        }
    }

    internal static float ApplyPrioritize(float weight, bool prioritized)
    {
        if (weight < 0f || weight > 1f)
        {
            Debug.LogWarning("Extra hauler target weight should be between 0 and 1!");
        }

        if (prioritized && weight >= 0.5f)
        {
            return weight + PriorityFactor;
        }

        return weight;
    }

    readonly record struct Entry(
        ExtraHaulerTargetRegistration Registration,
        FillInputWorkplaceBehavior Fill,
        bool Prioritized
    );

    sealed class RegistrationHandle(ExtraHaulerTargetService service, Inventory inventory) : IDisposable
    {
        bool disposed;

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            service.RemoveExtraTarget(inventory);
        }
    }
}
