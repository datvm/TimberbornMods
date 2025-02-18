namespace SuperCursor.Services.ObjectDescribers;

public interface IObjectDescriber
{
    void Describe(StringBuilder builder, BaseComponent component);
}
