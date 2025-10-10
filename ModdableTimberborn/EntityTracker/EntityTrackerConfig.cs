namespace ModdableTimberborn.EntityTracker
{
    public class EntityTrackerConfig : IModdableTimberbornRegistryConfig
    {
        public static readonly EntityTrackerConfig Instance = new();

        public void Configure(Configurator configurator, ConfigurationContext context)
        {
            configurator
                .BindSingleton<EntityTrackerController>()

                .MultiBindAndBindSingleton<IEntityTracker, CharacterTracker>()
                .MultiBindAndBindSingleton<IEntityTracker, WorkplaceTracker>()

                .BindTemplateModule(h => h
                    .AddDecorator<Character, CharacterTrackerComponent>()
                    .AddDecorator<WorkplaceSpec, WorkplaceTrackerComponent>()
                )
            ;

            foreach (var t in ModdableTimberbornRegistry.Instance.TrackedEntityTypes)
            {
                var targetType = typeof(DefaultEntityTracker<>).MakeGenericType(t);
                configurator.BindSingleton(targetType);
                configurator.MultiBind(typeof(IEntityTracker), targetType, toExisting: true);
            }
        }
    }
}

namespace ModdableTimberborn.Registry
{
    public partial class ModdableTimberbornRegistry
    {
        public bool EntityTrackerUsed { get; private set; }
        readonly HashSet<Type> trackedEntityTypes = [];
        public IReadOnlyCollection<Type> TrackedEntityTypes => trackedEntityTypes;

        public ModdableTimberbornRegistry UseEntityTracker()
        {
            if (EntityTrackerUsed) { return this; }

            EntityTrackerUsed = true;
            AddConfigurator(EntityTrackerConfig.Instance);

            return this;
        }

        public ModdableTimberbornRegistry TryTrack<TComp>()
            where TComp : BaseComponent
        {
            const string ErrorMessage = "Use {0} already injected (no need to call TryTrack<T>)";

            var t = typeof(TComp);
            if (t == typeof(CharacterTrackerComponent) || t == typeof(Character) || t == typeof(BotSpec) || t == typeof(BeaverSpec) || t == typeof(AdultSpec) || t == typeof(ChildSpec))
            {
                throw new InvalidOperationException(string.Format(ErrorMessage, nameof(CharacterTracker)));
            }
            else if (t == typeof(WorkplaceTrackerComponent) || t == typeof(WorkplaceSpec) || t == typeof(Workplace))
            {
                throw new InvalidOperationException(string.Format(ErrorMessage, nameof(WorkplaceTracker)));
            }

            trackedEntityTypes.Add(typeof(TComp));
            return this;
        }

    }
}