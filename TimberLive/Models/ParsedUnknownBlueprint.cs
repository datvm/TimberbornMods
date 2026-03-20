namespace TimberLive.Models;

public record ParsedUnknownBlueprint(HttpBlueprint Blueprint, string Path) : IParsedTemplateBlueprint
{
    public TemplateType Type => TemplateType.Unknown;
    public int Order => 0;
    public string TemplateName => "";
    public string DisplayNameLoc => "Unknown";

    public ParsedTemplateSpec TemplateSpec => throw new NotSupportedException();
}
