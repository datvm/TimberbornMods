namespace TimberUi.Helpers;

public class TemplateModuleHelper(Configurator configurator)
{

    readonly TemplateModule.Builder builder = new();

    public TemplateModuleHelper AddDecorator<TSubject, TDecorator>()
    {
        builder.AddDecorator<TSubject, TDecorator>();
        return this;
    }

    public void Bind()
    {
        configurator.MultiBind<TemplateModule>().ToProvider(builder.Build).AsSingleton();
    }

}

public class SimpleProvider<T>(T input) : IProvider<T>
{
    public T Get() => input;
}