namespace ModdableAlternativeBuildingStyle.Components;

// Implement this on the same building. It runs after a style is applied or restored.
public interface IAlternativeBuildingStyleListener
{
    void OnAlternativeStyleChanged(AlternativeBuildingStyleComponent building, string? style);
}
