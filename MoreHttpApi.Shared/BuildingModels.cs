namespace MoreHttpApi.Shared;

public record HttpBuilding(
    HttpEntityModel Entity,
    HttpNamedEntity Name,
    string LabelName,
    string TemplateName
)
{
    public HttpBuildingPausable? Pausable { get; set; }
    public HttpLabeledEntity? TemplateSpec { get; set; }
}

public record HttpGroupedBuildingTemplate(
    string TemplateName,
    HttpLabeledEntity? TemplateSpec,
    HttpBuilding[] Buildings
);

public record HttpGroupedBuildings(
    HttpGroupedBuildingTemplate[] Groups
);