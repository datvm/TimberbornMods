namespace ModdableTimberborn.DependencyInjection;

public class EditableBlueprint(string name)
{

    public string Name { get; set; } = name;
    public List<EditableBlueprint> Children { get; set; } = [];
    public List<ComponentSpec> Specs { get; set; } = [];

    public EditableBlueprint(Blueprint blueprint) : this(blueprint.Name)
    {
        Children = [.. blueprint.Children.Select(static q => new EditableBlueprint(q))];
        Specs = [.. blueprint.Specs];
    }

    public EditableBlueprint(string name, ComponentSpec spec) : this(name)
    {
        Specs.Add(spec);
    }

    public void TransformSpecs(Func<ComponentSpec, ComponentSpec?> transformer)
    {
        for (int i = 0; i < Specs.Count; i++)
        {
            var modified = transformer(Specs[i]);
            if (modified is not null)
            {
                Specs[i] = modified;
            }
        }
    }

    public void TransformSpec<T>(Func<T, T?> transform) where T : ComponentSpec
        => TransformSpecs(s => (s is T t) ? transform(t) : null);

    public T GetSpec<T>() where T : ComponentSpec => Specs.OfType<T>().First();

    public Blueprint ToBlueprint() => new(Name, Specs, [.. Children.Select(static q => q.ToBlueprint())]);
    public static implicit operator Blueprint(EditableBlueprint editableBlueprint) => editableBlueprint.ToBlueprint();

}
