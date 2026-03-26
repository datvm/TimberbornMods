namespace TimberLive.Components.Pages.Wiki.Components;

partial class FactionTemplates
{

    [Parameter, EditorRequired]
    public FactionTemplateCompilation Compilation { get; set; }

    [Parameter, EditorRequired]
    public FactionWithGroupedTemplates CurrentFaction { get; set; }

}
