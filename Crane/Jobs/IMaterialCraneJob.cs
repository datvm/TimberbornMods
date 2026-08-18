namespace Crane.Jobs;

public interface IMaterialCraneJob : ICraneJob
{
    IEnumerable<GoodAmount> GetRemainingMaterials();

    IEnumerable<GoodAmount> GetTotalMaterials();

    event EventHandler? MaterialsChanged;

    /// <returns>Amount actually added to the site.</returns>
    int AddMaterial(GoodAmount material);
}
