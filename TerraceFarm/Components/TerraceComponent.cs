namespace TerraceFarm.Components;

public record TerraceSpec : ComponentSpec;

[AddTemplateModule2(typeof(TerraceSpec))]
public class TerraceComponent(TerraceService service) : BaseComponent, IAwakableComponent, IFinishedStateListener
{
    const int CropTiltModifierOrder = 30;
    const float CropTiltDegrees = 45f;
    const float CropLiftHeight = 0.5f;

#nullable disable
    BlockObject bo;
#nullable enable

    PositionModifier? cropLiftModifier;

    public Vector3Int Coordinates => bo.Coordinates;

    public Orientation Orientation => bo.Orientation;

    /// <summary>Crop currently sharing this cell (visual pairing; may be unfinished terrace).</summary>
    public Growable? PairingCrop { get; private set; }

    /// <summary>Cluster size used for bonus (includes self). Only meaningful when <see cref="ContributesToBonus"/>.</summary>
    public int NeighborPairs { get; internal set; }

    public bool HasPairingCrop => PairingCrop is not null;

    /// <summary>Finished terrace with a paired crop — counts for growth bonus for self and neighbors.</summary>
    public bool ContributesToBonus => bo.IsFinished && PairingCrop is not null;

    public bool IsFinished => bo.IsFinished;

    public void Awake()
    {
        bo = GetComponent<BlockObject>();
    }

    public void AttachCrop(Growable g)
    {
        if (PairingCrop && PairingCrop != g)
        {
            DetachCrop();
        }

        PairingCrop = g;
        ApplyCropTilt(g);
        ApplyCropLift(g);
    }

    public void DetachCrop()
    {
        if (PairingCrop is not { } crop)
        {
            return;
        }

        GetOrAddTerraceCropRotationModifier(crop).Reset();
        cropLiftModifier?.Reset();
        cropLiftModifier = null;
        PairingCrop = null;
        NeighborPairs = 0;
    }

    /// <summary>
    /// Tips the crop so it stands perpendicular to the terrace face.
    /// Stairs/slope mesh: low end at local +Z (grid +Y at Cw0), high end at local -Z.
    /// Surface normal leans toward the low side, so the crop tips downhill.
    /// </summary>
    void ApplyCropTilt(Growable g)
    {
        // Building transform already includes Orientation (Cw0 = identity: +Z world = +Y grid).
        // Positive pitch around local right tips local up toward local +Z (downhill).
        var right = bo.Transform.right;
        GetOrAddTerraceCropRotationModifier(g).Set(Quaternion.AngleAxis(CropTiltDegrees, right));
    }

    void ApplyCropLift(Growable g)
    {
        cropLiftModifier ??= g.GetComponent<TransformController>().AddPositionModifier();
        cropLiftModifier.Set(new Vector3(0f, CropLiftHeight, 0f));
    }

    static RotationModifier GetOrAddTerraceCropRotationModifier(Growable g)
    {
        var ctrl = g.GetComponent<TransformController>();
        var ms = ctrl._rotationModifiers;
        if (!ms.TryGetValue(CropTiltModifierOrder, out var mod))
        {
            mod = ctrl.AddRotationModifier(CropTiltModifierOrder);
        }

        return mod;
    }

    public void OnEnterFinishedState()
    {
        service.OnTerraceFinishedStateChanged(this);
    }

    public void OnExitFinishedState()
    {
        // Construction exit / delete: clear bonus contribution while visual detach is handled by untrack.
        service.OnTerraceFinishedStateChanged(this);
    }
}
