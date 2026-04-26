
namespace BeaverChronicles.Helpers;

public static class BeaverChroniclesUtils
{
    public static readonly ImmutableArray<EventTriggerSource> AllTriggerSources 
        = [.. Enum.GetValues(typeof(EventTriggerSource)).Cast<EventTriggerSource>()];

    public static void Log(string msg) => Debug.Log($"[{nameof(BeaverChronicles)}] {msg}");

    extension(Configurator config)
    {

        public Configurator BindAllEvents(Assembly? assembly = default)
        {
            assembly ??= Assembly.GetCallingAssembly();

            foreach (var t in assembly.GetTypes())
            {
                if (typeof(IChronicleEvent).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && t.IsClass)
                {
                    config.BindSingleton(t);
                    config.MultiBind(typeof(IChronicleEvent), t, true);
                }
            }

            return config;
        }

    }

}
