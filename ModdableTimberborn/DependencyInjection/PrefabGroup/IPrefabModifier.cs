namespace ModdableTimberborn.DependencyInjection;

/// <summary>
/// A service that modifies prefabs right after they are loaded by the <see cref="PrefabGroupService.Load"/> based on the <see cref="PrefabSpec"/>. 
/// </summary>
public interface IPrefabModifier
{
    public int Order => 0;

    /// <summary>
    /// Determines whether this modifier should modify the given prefab based on its name and <see cref="PrefabSpec"/>.
    /// </summary>
    bool ShouldModify(string prefabName, PrefabSpec prefabSpec);

    /// <summary>
    /// Modifies the given prefab based on the <see cref="PrefabSpec"/> and returns the modified prefab.
    /// Note that you are given a copy so it does not persist across game loads.
    /// Return null if no modification is made.
    /// </summary>
    GameObject? Modify(GameObject prefab, PrefabSpec prefabSpec, GameObject original);
}
