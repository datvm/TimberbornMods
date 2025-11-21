namespace ConfigurableTopBar.Models;

public class EditableGoodGroupSpec(string id, string icon, string name, bool isBuiltIn = false)
{
    public const string SpecialGroupId = "Ungrouped";

    public string Id { get; set; } = id;
    public string Icon { get; set; } = icon;
    public string Name { get; set; } = name;

    public bool IsBuiltIn { get; set; } = isBuiltIn;

    public bool SingleResourceGroup => Goods.Count == 1;

    public List<EditableGoodSpec> Goods { get; } = [];

    public static EditableGoodGroupSpec CreateSpecialGroup(ILoc t) => new(
        SpecialGroupId,
        GoodSpriteProvider.QuestionMarkPath,
        t.T("LV.CTB.Ungrouped"),
        true
    );

}
