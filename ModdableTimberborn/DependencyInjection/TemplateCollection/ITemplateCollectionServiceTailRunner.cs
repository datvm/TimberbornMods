namespace ModdableTimberborn.DependencyInjection;

/// <summary>
/// A service that runs right after the <see cref="TemplateCollectionService.Load"/> was executed.
/// If it is also an <see cref="ILoadableSingleton"/>, its <see cref="ILoadableSingleton.Load"/> will be executed before <see cref="TemplateCollectionService.Load"/>.
/// </summary>
public interface ITemplateCollectionServiceTailRunner
{
    int Order => 0;

    /// <summary>
    /// Runs right after the <see cref="TemplateCollectionService.Load"/> was executed.
    /// </summary>
    void Run(TemplateCollectionService templateCollectionService);
}
