namespace RamPump.Components;

public class RamPumpModelModifier : BaseComponent, IAwakableComponent
{
    static readonly Vector3 RotationPivot = new(.5f, 0f, .5f);

    public void Awake()
    {
        var topModels = GameObject.GetAllChildren().Where(obj => obj.name == "#Top");

        foreach (var model in topModels)
        {
            var t = model.transform;
            var center = t.position + t.TransformDirection(RotationPivot);
            t.RotateAround(center, Vector3.up, 90f);
            t.position += Vector3.up;
        }
    }

}
