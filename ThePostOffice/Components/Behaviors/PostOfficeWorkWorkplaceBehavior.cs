namespace ThePostOffice.Components.Behaviors;

public class PostOfficeWorkWorkplaceBehavior : WorkplaceBehavior, IAwakableComponent
{
    public const float WorkChunkHours = 0.25f;

    PostOfficeActiveParcel active = null!;
    Workplace workplace = null!;
    Enterable enterable = null!;
    Emptiable? emptiable;

    public void Awake()
    {
        active = GetComponent<PostOfficeActiveParcel>();
        workplace = GetComponent<Workplace>();
        enterable = GetComponent<Enterable>();
        emptiable = GetComponent<Emptiable>();
    }

    public override Decision Decide(BehaviorAgent agent)
    {
        var emptying = emptiable;
        if ((emptying && emptying!.IsMarkedForEmptying) || !active.IsRunnable)
        {
            return Decision.ReleaseNow();
        }

        var walk = agent.GetComponent<WalkInsideExecutor>();
        switch (walk.Launch(enterable))
        {
            case ExecutorStatus.Success:
                var work = agent.GetComponent<PostOfficeWorkExecutor>();
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
