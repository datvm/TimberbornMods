namespace BuildingHP.Components;

public class BuildingHPComponentSpec : BaseComponent
{
    public const int DefaultBaseDurability = 10;

    [SerializeField]
    public int _baseDurability = DefaultBaseDurability;

    [SerializeField]
    public bool _noMaterialDurability = false;

    public int BaseDurability => _baseDurability;
    public bool NoMaterialDurability => _noMaterialDurability;

}
