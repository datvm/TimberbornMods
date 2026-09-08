namespace Timberborn.BaseComponentSystem
{
    public static class BuffHelpers
    {

        public static TrackingEntityHelper BindTrackingEntities(this Configurator configurator) => new(configurator);

        public static TemplateModuleHelper BindTemplateModule(this Configurator configurator) => new(configurator);

        public static BuffableComponent GetBuffable<T>(this T comp) where T : BaseComponent
        {
            var result = comp.GetComponentFast<BuffableComponent>();
            return result;
        }

        public static IEnumerable<BuffInstance> GetBuffs<T>(this T comp) where T : BaseComponent => comp.GetBuffable().Buffs;

        public static IEnumerable<T> GetBuffEffects<T>(this BuffInstance instance, bool includeDisabled = false) where T : IBuffEffect 
            => (includeDisabled || instance.Active) ? instance.Effects.OfType<T>() : [];

        public static string GetHumanFriendlyId<T>(this T b) where T : IBuff
        {
            return $"Buff {b.Name} (Id {b.Id}, Type {b.GetType().FullName})";
        }

        public static bool IsBeaver(this BaseComponent comp, BeaverTarget target = default)
        {
            return target switch
            {
                BeaverTarget.All => comp.GetComponentFast<BeaverSpec>() is not null,
                BeaverTarget.Adult => comp.GetComponentFast<AdultSpec>() is not null,
                BeaverTarget.Child => comp.GetComponentFast<ChildSpec>() is not null,
                _ => false,
            };
        }

        public static BotSpec? IsBot(this BaseComponent comp) => comp.GetComponentFast<BotSpec>();

    }
}

namespace Timberborn.Persistence
{

    public static class BuffHelpers
    {

        public static void Set(this IObjectSaver saver, PropertyKey<string> key, long value)
        {
            saver.Set(key, value.ToString()); // Don't remove ToString() or it will call the wrong overload
        }

        public static long GetLong(this IObjectLoader loader, PropertyKey<string> key)
        {
            return long.Parse(loader.Get(key));
        }

        public static void SetBuffEntityId(this IObjectSaver saver, long id)
        {
            saver.Set(BuffDebuffUtils.IdKey, id);
        }

        public static long GetBuffEntityId(this IObjectLoader loader)
        {
            return loader.GetLong(BuffDebuffUtils.IdKey);
        }

    }
}