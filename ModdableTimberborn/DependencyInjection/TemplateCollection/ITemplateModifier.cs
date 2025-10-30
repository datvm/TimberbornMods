namespace ModdableTimberborn.DependencyInjection;

/// <summary>
/// A service that modifies templates right after they are loaded by the <see cref="TemplateCollectionService.Load"/> based on the <see cref="TemplateSpec"/>. 
/// </summary>
public interface ITemplateModifier
{
    int Order => 0;

    /// <summary>
    /// Determines whether this modifier should modify the given template based on its name and <see cref="TemplateSpec"/>.
    /// </summary>
    bool ShouldModify(string blueprintName, string templateName, TemplateSpec originalTemplateSpec);

    /// <summary>
    /// Modifies the given template based on the <see cref="TemplateSpec"/> and returns the modified template.
    /// Return null if no modification is made.
    /// </summary>
    EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original);
}
