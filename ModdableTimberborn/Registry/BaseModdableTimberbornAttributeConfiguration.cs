namespace ModdableTimberborn.Registry;

public abstract class BaseModdableTimberbornAttributeConfiguration : BaseModdableTimberbornConfiguration
{

    public virtual Assembly Assembly => GetType().Assembly;
    public virtual Scope DefaultScope => Scope.Singleton;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator.BindAttributes(context, Assembly, DefaultScope);
    }

}
