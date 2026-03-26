namespace TimberLive.Components.Pages.Characters.Components;

partial class WellbeingDetails
{

    [Parameter, EditorRequired]
    public HttpCharacterDetailed Character { get; set; }

    CharacterGroupedNeed[] CompileNeeds()
    {
        Dictionary<string, CharacterGroupedNeed> result = [];

        var grps = CommonData.NeedsGroup;
        var needs = CommonData.Needs;

        foreach (var need in Character.Needs)
        {
            var spec = needs[need.Id];
            var grp = grps[spec.NeedGroupId];

            var entry = result.GetOrAdd(grp.Id, _ => new(grp, []));
            entry.Needs.Add((spec, need));
        }

        return [.. result.Values.OrderBy(q => q.Group.Order)];
    }

    readonly record struct CharacterGroupedNeed(
        ParsedNeedGroupSpec Group, 
        List<(ParsedNeedSpec Spec, HttpCharacterNeed Value)> Needs
    );

}