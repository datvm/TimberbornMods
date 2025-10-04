namespace ModdableTimberborn.DependencyInjection;

/// <summary>
/// A service that runs right after the <see cref="PrefabGroupService.Load"/> was executed.
/// If it is also an <see cref="ILoadableSingleton"/>, its <see cref="ILoadableSingleton.Load"/> will be executed before <see cref="PrefabGroupService.Load"/>.
/// </summary>
public interface IPrefabGroupServiceTailRunner
{
    /// <summary>
    /// Runs right after the <see cref="PrefabGroupService.Load"/> was executed.
    /// </summary>
    void Run(PrefabGroupService prefabGroupService);
}
