global using Timberborn.MechanicalSystem;
global using Timberborn.Workshops;

namespace BuffDebuff;

public class TrackingEntityHelper(Configurator configurator)
{
    public static readonly ImmutableHashSet<Type> BuilderBuildingTypes = [typeof(DistrictCenter), typeof(BuilderHubSpec)];

    public TrackingEntityHelper Track<T>() where T : BaseComponent
    {
        configurator.MultiBind<ITrackingEntities>()
            .To<TrackingEntities<T>>().AsSingleton();

        return this;
    }

    public TrackingEntityHelper Track(params Type[] types)
    {
        configurator.MultiBind<ITrackingEntities>()
            .ToProvider(() => new TrackingEntities(types)).AsSingleton();

        return this;
    }

    public TrackingEntityHelper TrackBuilderBuildings()
    {
        return Track([.. BuilderBuildingTypes]);
    }

    public TrackingEntityHelper TrackWorkplace()
    {
        return Track<Workplace>();
    }

    public TrackingEntityHelper TrackPowered()
    {
        return Track<MechanicalNode>();
    }

    public TrackingEntityHelper TrackManufactorer()
    {
        return Track<Manufactory>();
    }

}

public interface ITrackingEntities
{
    IEnumerable<Type> TrackingTypes { get; }
}

public class TrackingEntities(IEnumerable<Type> types) : ITrackingEntities
{
    public IEnumerable<Type> TrackingTypes { get; } = types;
}

public class TrackingEntities<T>() : TrackingEntities([typeof(T)]);
