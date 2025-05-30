namespace Bindito.Core;

public static partial class UiBuilderExtensions
{

    /// <summary>
    /// Create a <see cref="TemplateModuleHelper"/> to bind a template module with fluent API style.
    /// </summary>
    /// <remarks>
    /// Remember to call <see cref="TemplateModuleHelper.Bind"/> after configuring the template module to apply the bindings.
    /// Use <see cref="BindTemplateModule(Configurator, Func{TemplateModuleHelper, TemplateModuleHelper})"/> to configure and bind in one go.
    /// </remarks>
    public static TemplateModuleHelper BindTemplateModule(this Configurator configurator) => new(configurator);

    /// <summary>
    /// Configures and binds a template module to the configurator.
    /// </summary>
    /// <remarks>This method provides a fluent interface for configuring and binding a template module.  The
    /// <paramref name="configure"/> delegate is used to customize the behavior of the template module before it is
    /// bound. You do not need to call Bind on this one as it's called for you.</remarks>
    /// <param name="configure">A delegate that allows customization of the <see cref="TemplateModuleHelper"/> used to configure the template
    /// module. The delegate is invoked with a <see cref="TemplateModuleHelper"/> instance, and any modifications made
    /// to it will be applied before binding.</param>
    public static Configurator BindTemplateModule(this Configurator configurator, Func<TemplateModuleHelper, TemplateModuleHelper> configure)
    {
        var helper = configurator.BindTemplateModule();
        configure(helper);
        helper.Bind();

        return configurator;
    }

    /// <summary>
    /// Creates a new <see cref="MassRebindingHelper"/> instance to facilitate mass rebinding operations which is faster and more convenient.
    /// </summary>
    /// <remarks>
    /// You have to call <see cref="MassRebindingHelper.Bind"/> to apply the rebinding operations.
    /// Use <see cref="MassRebind(Configurator, Action{MassRebindingHelper})"/> to configure and bind in one go.
    /// </remarks>
    /// <returns>A <see cref="MassRebindingHelper"/> instance configured with the specified <paramref name="configurator"/>.</returns>
    public static MassRebindingHelper MassRebind(this Configurator configurator) => new(configurator);

    /// <summary>
    /// Performs a mass rebinding operation which is faster and more convenient than rebinding one by one.
    /// </summary>
    /// <param name="configure">A delegate that configures the <see cref="MassRebindingHelper"/> used for the rebinding process. The delegate
    /// allows customization of the rebinding behavior.</param>
    public static Configurator MassRebind(this Configurator configurator, Action<MassRebindingHelper> configure)
    {
        var helper = configurator.MassRebind();
        configure(helper);
        return helper.Bind();
    }

}
