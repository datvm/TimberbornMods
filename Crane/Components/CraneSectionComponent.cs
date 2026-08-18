namespace Crane.Components;

[AddTemplateModule2(typeof(CraneSectionSpec))]
public class CraneSectionComponent(
    CraneStructureService craneStructureService,
    GoodRecoveryRateService goodRecoveryRateService
) : BaseComponent, IRecoverableGoodMultiplier, IAwakableComponent, IUnfinishedStateListener, IFinishedStateListener, IDeletableEntity
{

    BlockObject bo = null!;
    ConstructionSite constructionSite = null!;
    public Vector3Int Coordinates => bo.Coordinates;
    public bool IsFinished => bo.IsFinished;

    public CraneTower? Tower { get; internal set; }
    public CraneComponent? Crane => Tower?.Crane;

    public void Awake()
    {
        bo = GetComponent<BlockObject>();
        constructionSite = GetComponent<ConstructionSite>();
    }

    public void OnEnterUnfinishedState()
    {
        if (TryGetComponent<BlockObjectInit>(out _))
        {
            var prioritizable = GetComponent<BuilderPrioritizable>();
            if (prioritizable)
            {
                prioritizable.SetPriority(Priority.VeryHigh);
            }
        }

        craneStructureService.RefreshCraneSectionStructure(this);
    }

    public void OnExitUnfinishedState() { }

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
        var crane = Crane ?? craneStructureService.FindCraneOfSection(this);
        if (crane is not null)
        {
            craneStructureService.RefreshCraneStructure(crane, this);
        }
    }

    public float GetMultiplierForInventory(Inventory inventory)
    {
        if (!bo.IsFinished || inventory != constructionSite.Inventory)
        {
            return 1f;
        }

        var rate = goodRecoveryRateService.DemolishableRecoveryRate;
        if (rate <= 0f)
        {
            return 1f;
        }

        return 1f / rate;
    }

}