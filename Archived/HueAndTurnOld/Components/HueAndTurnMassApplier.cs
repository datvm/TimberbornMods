namespace HueAndTurn.Components;

public class HueAndTurnMassApplier(EntityRegistry entities, ILoc t, DialogBoxShower diagShower)
{

    public void Confirm(Action onConfirm)
    {
        diagShower.Create()
            .SetMessage(t.T("LV.HNT.AllConfirm"))
            .SetConfirmButton(onConfirm)
            .SetDefaultCancelButton()
            .Show();
    }

    public void CopyColor(IHueAndTurnComponent comp) => ApplyToAll(comp, (c, props) =>
    {
        c.SetColor(props.Color);
        c.SetTransparency(props.Transparency);
    });

    public void CopyAllProps(IHueAndTurnComponent comp) => ApplyToAll(comp, (c, props) =>
    {
        c.ApplyProperties(props);
    });

    public void RandomizeRotations(IHueAndTurnComponent comp) => ApplyToAll(comp, (c, _) =>
    {
        var rotation = UnityEngine.Random.RandomRangeInt(-180, 180);
        c.SetRotationWithPivot(rotation, Vector2Int.zero);
    });

    public void Reset(IHueAndTurnComponent comp) => ApplyToAll(comp, (c, _) => c.Reset());

    void ApplyToAll(IHueAndTurnComponent comp, Action<IHueAndTurnComponent, ReadOnlyHueAndTurnProperties> action)
    {
        if (!comp.PrefabSpec) { return; }
        var name = comp.PrefabName;
        var props = comp.Properties;

        foreach (var e in entities.Entities)
        {
            var hat = e.GetComponentFast<HueAndTurnComponent>();
            if (!hat || hat.PrefabName != name) { continue; }

            action(hat, props);
        }
    }

}
