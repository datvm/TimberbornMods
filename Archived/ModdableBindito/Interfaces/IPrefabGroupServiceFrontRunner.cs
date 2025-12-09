namespace ModdableBindito;

/// <summary>
/// Multibound services with this interface will run before <see cref="PrefabGroupService"/>.
/// <remarks>It must not depend on <see cref="PrefabGroupService"/>.</remarks>
/// </summary>
public interface IPrefabGroupServiceFrontRunner
{
    /// <summary>
    /// This method runs right after <see cref="PrefabGroupService.Load"/> where <see cref="PrefabGroupService.AllPrefabs"/> is available.
    /// It is also run after all the <see cref="IPrefabModifier.ModifyPrefab(GameObject)"/> has run.
    /// </summary>
    void AfterPrefabLoad(PrefabGroupService prefabGroupService);
}

/// <summary>
/// Indicate that this service can modify the prefabs right after all the prefabs are loaded.
/// </summary>
public interface IPrefabModifier
{
    /// <summary>
    /// The priority in which this service will be called. Higher ones are called first.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Modify a prefab.
    /// </summary>
    /// <param name="prefab">The current prefab to be modified. Could have been modified by other services.</param>
    /// <returns>The modified prefab.</returns>
    GameObject ModifyPrefab(GameObject prefab);
}
