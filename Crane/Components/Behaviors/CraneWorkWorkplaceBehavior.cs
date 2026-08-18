namespace Crane.Components.Behaviors;

public class CraneWorkWorkplaceBehavior : WorkplaceBehavior, IAwakableComponent
{
    public const float WorkChunkHours = 0.25f;

    CraneWorkshop workshop = null!;
    Workplace workplace = null!;
    Enterable enterable = null!;

    public void Awake()
    {
        workshop = GetComponent<CraneWorkshop>();
        workplace = GetComponent<Workplace>();
        enterable = GetComponent<Enterable>();
    }

    public override Decision Decide(BehaviorAgent agent)
    {
        if (workshop.IsEmptying || !workshop.HasWork)
        {
            return Decision.ReleaseNow();
        }

        var walk = agent.GetComponent<WalkInsideExecutor>();
        switch (walk.Launch(enterable))
        {
            case ExecutorStatus.Success:
                var work = agent.GetComponent<CraneWorkExecutor>();
                if (!work.Launch(WorkChunkHours))
                {
                    return Decision.ReleaseNextTick();
                }

                return Decision.ReleaseWhenFinished(work);
            case ExecutorStatus.Failure:
                workplace.UnassignWorker(agent.GetComponent<Worker>());
                return Decision.ReleaseNextTick();
            case ExecutorStatus.Running:
                return Decision.ReleaseWhenFinished(walk);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

}
