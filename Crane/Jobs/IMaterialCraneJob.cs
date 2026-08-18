namespace Crane.Jobs;

public interface IMaterialCraneJob : ICraneJob
{
    IEnumerable<GoodAmount> GetMaterials();

    event EventHandler? MaterialsChanged;

    /// <returns>Amount actually added to the site.</returns>
    int AddMaterial(GoodAmount material);
}
