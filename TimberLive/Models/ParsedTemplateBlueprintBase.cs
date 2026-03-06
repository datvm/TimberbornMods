namespace TimberLive.Models;

public abstract class ParsedTemplateBlueprintBase(HttpBlueprint blueprint, string path) : IParsedLabeledTemplateBlueprint
{
    public ParsedTemplateSpec TemplateSpec { get; } = blueprint.GetComponent<ParsedTemplateSpec>();
    public ParsedLabeledEntitySpec LabeledEntitySpec { get; } = blueprint.GetComponent<ParsedLabeledEntitySpec>();

    public HttpBlueprint Blueprint => blueprint;
    public string Path => path;

    public abstract TemplateType Type { get; }
    public abstract int Order { get; }

    public string TemplateName => TemplateSpec.TemplateName;
    public string DisplayNameLoc => LabeledEntitySpec.DisplayNameLocKey;
    public string ImagePath => LabeledEntitySpec.Icon;
}
