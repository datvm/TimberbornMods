namespace CopyNameToo.Components;

[AddTemplateModule2(typeof(NamedEntity))]
public class CopiableNameComponent : BaseComponent, IAwakableComponent, IDuplicable<CopiableNameComponent>, IDuplicable
{

    bool IDuplicable.IsDuplicable => IsEditable;
    NamedEntity namedEntity = null!;

    public string EntityName => namedEntity.EntityName;
    public bool IsEditable => namedEntity && namedEntity.IsEditable;

    public void Awake()
    {
        namedEntity = GetComponent<NamedEntity>();
    }

    public void DuplicateFrom(CopiableNameComponent source) => SetEntityName(source.EntityName);

    public bool SetEntityName(string entityName)
    {
        if (!IsEditable) { return false; }

        namedEntity.SetEntityName(entityName);
        return true;
    }

}
