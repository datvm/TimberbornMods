namespace TImprove4Modders.DevModules;

public class ComponentModule(EventBus eb) : IDevModule, ILoadableSingleton
{
    SelectableObject? currObj;

    public DevModuleDefinition GetDefinition()
    {
        return new DevModuleDefinition.Builder()
            .AddMethod(DevMethod.Create("Components: Print all types", () => PrintComponents(false)))
            .AddMethod(DevMethod.Create("Components: Print all with values", () => PrintComponents(true)))
            .Build();
    }

    public void Load()
    {
        eb.Register(this);
    }

    [OnEvent]
    public void OnObjectSelected(SelectableObjectSelectedEvent ev)
    {
        currObj = ev.SelectableObject;
    }

    [OnEvent]
    public void OnObjectDeselected(SelectableObjectUnselectedEvent ev)
    {
        currObj = null;
    }

    static readonly BindingFlags ComponentMembersFlag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
    public void PrintComponents(bool detailed)
    {
        if (!currObj)
        {
            Debug.LogWarning("No entity selected.");
            return;
        }

        foreach (var c in currObj.AllComponents)
        {
            var type = c.GetType();
            Debug.Log(type.FullName);

            if (detailed)
            {
                var fields = type.GetFields(ComponentMembersFlag);
                foreach (var field in fields)
                {
                    var value = field.GetValue(c);
                    Debug.Log($"  F: {field.Name}: {value}");
                }

                var properties = type.GetProperties(ComponentMembersFlag);
                foreach (var property in properties)
                {
                    if (property.CanRead)
                    {
                        var value = property.GetValue(c);
                        Debug.Log($"  P: {property.Name}: {value}");
                    }
                }
            }
        }
    }

}
