namespace BeaverController.Components;

[AddTemplateModule2(typeof(Character))]
public class CharacterMoveBehavior : RootBehavior, IAwakableComponent
{

    CharacterControllerComponent controller = null!;
    WalkToPositionExecutor walk = null!;

    public void Awake()
    {
        controller = GetComponent<CharacterControllerComponent>();
        walk = GetComponent<WalkToPositionExecutor>();

        var manager = GetComponent<BehaviorManager>();
        if (!manager || manager._rootBehaviors.Contains(this)) { return; }

        manager._rootBehaviors.Insert(0, this);
    }

    public override Decision Decide(BehaviorAgent agent)
    {
        if (!controller.HasOrder || !walk)
        {
            return Decision.ReleaseNow();
        }

        var destination = CoordinateSystem.GridToWorld(controller.OrderDestination);
        return walk.Launch(destination) switch
        {
            ExecutorStatus.Success => OnArrived(),
            ExecutorStatus.Failure => OnFailed(),
            ExecutorStatus.Running => Decision.ReturnWhenFinished(walk),
            _ => throw new ArgumentOutOfRangeException(nameof(walk)),
        };
    }

    Decision OnArrived()
    {
        if (controller.KeepAfterMove)
        {
            controller.BeginHold(ControllableCharacterAnimations.DefaultAnimation);
            return Decision.ReturnNextTick();
        }

        controller.CompleteOrder();
        return Decision.ReleaseNow();
    }

    Decision OnFailed()
    {
        controller.FailOrder();
        return Decision.ReleaseNow();
    }
}
