namespace HueAndTurn.Components;

public class HueAndTurnMassApplier(EntityRegistry entities, ILoc t, DialogBoxShower diagShower)
{

    static void DoNothing() { }

    public void Confirm(Action onConfirm)
    {
        diagShower.Create()
            .SetMessage(t.T("LV.HNT.AllConfirm"))
            .SetConfirmButton(onConfirm)
            .SetCancelButton(DoNothing)
            .Show();
    }

    public void CopyColor(HueAndTurnComponent comp)
    {
        var o = comp.Properties.Color;
        var transparency = comp.Properties.Transparency;
        ApplyToAll(comp, c =>
        {
            c.Properties.Color = o;
            c.Properties.Transparency = transparency;
            c.ApplyColor();
            c.ApplyTransparency();
        });
    }

    public void CopyAllProps(HueAndTurnComponent comp)
    {
        var props = comp.Properties;

        ApplyToAll(comp, c =>
        {
            c.ApplyProperties(props);
        });
    }

    public void RandomizeRotations(HueAndTurnComponent comp)
    {
        ApplyToAll(comp, c =>
        {
            c.Properties.RotationPivot = Vector2Int.zero;
            c.Properties.Rotation = UnityEngine.Random.RandomRangeInt(-180, 180);
            c.ApplyRepositioning();
        });
    }

    public void Reset(HueAndTurnComponent comp)
    {
        ApplyToAll(comp, c =>
        {
            c.Reset();
        });
    }

    void ApplyToAll(HueAndTurnComponent comp, Action<HueAndTurnComponent> action)
    {
        if (!comp.PrefabSpec) { return; }
        var name = comp.PrefabName;

        foreach (var e in entities.Entities)
        {
            var hat = e.GetComponentFast<HueAndTurnComponent>();
            if (!hat || hat.PrefabName != name) { continue; }

            action(hat);
        }
    }

}
