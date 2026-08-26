namespace CraneHeads.Components;

[AddTemplateModule2(typeof(CraneHeadTrebuchetSpec))]
public class CraneHeadTrebuchetModel(
    TrebuchetTrajectoryService trajectory
) : BaseComponent, IAwakableComponent, IPostInitializableEntity, IPostPlacementChangeListener,
    IFinishedStateListener, IModelUpdater, IDeletableEntity
{
    CraneHeadTrebuchet trebuchet = null!;
    CraneHeadTrebuchetLauncher launcher = null!;
    CraneHeadComponent head = null!;
    Transform? turret;
    Transform? barrel;
    Quaternion turretRest;
    Quaternion barrelRest;
    Vector3Int? preview;

    public void Awake()
    {
        trebuchet = GetComponent<CraneHeadTrebuchet>();
        launcher = GetComponent<CraneHeadTrebuchetLauncher>();
        head = GetComponent<CraneHeadComponent>();
        BindTransforms();
    }

    public void PostInitializeEntity()
    {
        launcher.TargetChanged += OnTargetChanged;
        head.CraneChanged += OnCraneChanged;
        Aim();
    }

    public void DeleteEntity()
    {
        launcher.TargetChanged -= OnTargetChanged;
        head.CraneChanged -= OnCraneChanged;
    }

    public void OnPostPlacementChanged() => Aim();

    public void OnEnterFinishedState() => Aim();

    public void OnExitFinishedState() => ResetAim();

    public void UpdateModel() => Aim();

    public void SetPreview(Vector3Int? dest)
    {
        preview = dest;
        Aim();
    }

    void OnTargetChanged(object sender, EventArgs e)
    {
        if (preview is null)
        {
            Aim();
        }
    }

    void OnCraneChanged(object sender, EventArgs e) => Aim();

    void BindTransforms()
    {
        var spec = GetComponent<CraneHeadTrebuchetSpec>();
        if (!string.IsNullOrEmpty(spec.Turret))
        {
            turret = GameObject.FindChildTransform(spec.Turret);
            if (turret)
            {
                turretRest = turret.localRotation;
            }
        }

        if (!string.IsNullOrEmpty(spec.Barrel))
        {
            barrel = GameObject.FindChildTransform(spec.Barrel);
            if (barrel)
            {
                barrelRest = barrel.localRotation;
            }
        }
    }

    void Aim()
    {
        var dest = preview ?? launcher.Target;
        if (dest is not { } tile || !turret)
        {
            ResetAim();
            return;
        }

        var tangent = trajectory.InitialWorldDirection(trebuchet.Origin, tile, trebuchet.PeakDelta);
        var parent = turret.parent;
        var local = parent
            ? parent.InverseTransformDirection(tangent)
            : tangent;
        var flat = new Vector3(local.x, 0f, local.z);
        if (flat.sqrMagnitude < 0.0001f)
        {
            ResetAim();
            return;
        }

        turret.localRotation = Quaternion.LookRotation(flat);
        if (!barrel)
        {
            return;
        }

        var elevation = Mathf.Atan2(local.y, flat.magnitude) * Mathf.Rad2Deg;
        // Firework barrel: local +Z is the muzzle. Euler(-90) points it up;
        // Euler(-elevation) tilts that up-pose toward turret forward along the curve.
        barrel.localRotation = Quaternion.Euler(-elevation, 0f, 0f);
    }

    void ResetAim()
    {
        if (turret)
        {
            turret.localRotation = turretRest;
        }

        if (barrel)
        {
            barrel.localRotation = barrelRest;
        }
    }
}
