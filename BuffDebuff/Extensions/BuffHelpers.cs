namespace Timberborn.BaseComponentSystem
{
    public static class BuffHelpers
    {

        public static BuffableComponent GetBuffable<T>(this T comp) where T : BaseComponent
        {
            var result = comp.GetComponentFast<BuffableComponent>();
            return result;
        }


        public static IEnumerable<BuffInstance> GetBuffs<T>(this T comp) where T : BaseComponent => comp.GetBuffable().Buffs;

        public static IEnumerable<T> GetBuffs<T>(this BaseComponent comp) where T : BuffInstance => GetBuffable(comp).GetBuffs<T>();
        public static IEnumerable<T> GetBuffEffects<T>(this BaseComponent comp, bool includeDisabled = false) where T : IBuffEffect => GetBuffable(comp).GetEffects<T>(includeDisabled);

        public static string GetHumanFriendlyId<T>(this T b) where T : IBuff
        {
            return $"Buff {b.Name} (Id {b.Id}, Type {b.GetType().FullName})";
        }

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