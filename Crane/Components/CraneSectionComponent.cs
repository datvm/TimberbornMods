namespace Crane.Components;

[AddTemplateModule2(typeof(CraneSectionSpec))]
public class CraneSectionComponent(
    CraneStructureService craneStructureService
) : BaseComponent, IRecoverableGoodMultiplier, IAwakableComponent, IFinishedStateListener, IDeletableEntity
{

    BlockObject bo = null!;
    public Vector3Int Coordinates => bo.Coordinates;
    public bool IsFinished => bo.IsFinished;

    public CraneTower? Tower { get; internal set; }
    public CraneComponent? Crane => Tower?.Crane;

    public void Awake()
    {
        bo = GetComponent<BlockObject>();
    }

    public void OnEnterFinishedState()
    {
        craneStructureService.RefreshCraneSectionStructure(this);
    }

    public void OnExitFinishedState()
    {
        craneStructureService.RefreshCraneSectionStructure(this);
    }

    public void DeleteEntity()
    {
        if (Tower is { } tower)
        {
            craneStructureService.RefreshCraneStructure(tower.Crane);
        }
        else
        {
            craneStructureService.RefreshCraneSectionStructure(this);
        }
    }

    // Always fully refund
    public float GetMultiplierForInventory(Inventory inventory) => 1f;

}