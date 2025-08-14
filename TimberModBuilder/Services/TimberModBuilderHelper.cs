namespace TimberModBuilder.Services;

public static class TimberModBuilderHelper
{
    public const string AppendPostfix = "#append";

    public static string Append(string name) => name + AppendPostfix;

    public static Dictionary<string, object> ToDictionary(
        object obj,
        HashSet<string>? properties = null,
        HashSet<string>? appendProperties = null
    )
    {
        var dict = new Dictionary<string, object>();
        var props = obj.GetType().GetProperties();

        foreach (var prop in props)
        {
            var value = prop.GetValue(obj);

            if (properties is not null && !properties.Contains(prop.Name)) { continue; }

            var append = appendProperties is not null && appendProperties.Contains(prop.Name);
            var propName = prop.Name + (append ? AppendPostfix : "");
            dict[propName] = value;
        }

        return dict;
    }

}
