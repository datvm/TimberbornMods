namespace TimberLive.Models;

public interface IParsedTemplateBlueprint
{
    string Path { get; }
    HttpBlueprint Blueprint { get; }
    TemplateType Type { get; }
    ParsedTemplateSpec TemplateSpec { get; }
    int Order { get; }
    string TemplateName { get; }
    string DisplayNameLoc { get; }
}

public interface IParsedLabeledTemplateBlueprint : IParsedTemplateBlueprint
{
    ParsedLabeledEntitySpec LabeledEntitySpec { get; }
    string ImagePath { get; }
}

public enum TemplateType
{
    Building,
    Crop,
    Tree,
    Unknown
}