namespace ModdableTimberborn.Registry;

public class ModdableTimberbornRegistry
{
    public static readonly ModdableTimberbornRegistry Instance = new();

    readonly HashSet<KeyValuePair<ConfigurationContext, Action<Configurator>>> configurators = [];

    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        foreach (var (ctx, c) in configurators)
        {
            if ((ctx & context) != 0)
            {
                c(configurator);
            }
        }
    }

    public bool MechanicalSystemUsed { get; private set; }
    public ModdableTimberbornRegistry UseMechanicalSystem()
    {
        if (MechanicalSystemUsed) { return this; }

        MechanicalSystemUsed = true;
        configurators.Add(new(ConfigurationContext.Game, config =>
        {

        }));

        return this;
    }

}
