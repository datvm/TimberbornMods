namespace TimberPipes.Components;

[AddTemplateModule2(typeof(ValvePipeSpec))]
public class ValvePipeModel(ValvePipeService service)
    : BaseComponent, IAwakableComponent, IPostInitializableEntity, IInitializablePreview,
        IPostPlacementChangeListener, IFinishedStateListener, IPreviewSelectionListener
{
#nullable disable
    BuildingPipe pipe;
    BuildingPipeSpec spec;
    BlockObject blockObject;
    GameObject straight;
    GameObject building;
    GameObject ringIn;
    GameObject ringOut;
#nullable enable

    public void Awake()
    {
        pipe = GetComponent<BuildingPipe>();
        spec = GetComponent<BuildingPipeSpec>();
        blockObject = GetComponent<BlockObject>();
        var finished = GameObject.FindChild("#Finished");
        if (!finished || spec is null)
        {
            DisableComponent();
            return;
        }

        var straightTransform = finished.transform.Find("#Straight");
        var buildingTransform = finished.transform.Find("#Building");
        if (!straightTransform || !buildingTransform)
        {
            DisableComponent();
            return;
        }

        straight = straightTransform.gameObject;
        building = buildingTransform.gameObject;
        ringIn = NamedChild(buildingTransform, "RingIn");
        ringOut = NamedChild(buildingTransform, "RingOut");
    }

    public void PostInitializeEntity()
    {
        Refresh();
    }

    public void InitializePreview()
    {
        Refresh();
    }

    public void OnPostPlacementChanged()
    {
        Refresh();
    }

    public void OnEnterFinishedState()
    {
        Refresh();
    }

    public void OnExitFinishedState()
    {
    }

    public void OnPreviewSelect()
    {
        Refresh();
    }

    public void OnPreviewUnselect()
    {
    }

    public void Refresh()
    {
        if (!Enabled || spec is null)
        {
            return;
        }

        var visual = ReadVisual();
        var useBuilding = visual is not null;
        straight.SetActive(!useBuilding);
        building.SetActive(useBuilding);
        if (visual is not { } v)
        {
            return;
        }

        if (v.Yaw == 180f)
        {
            building.transform.SetLocalPositionAndRotation(new(1f, 0f, 1f), Quaternion.Euler(0f, 180f, 0f));
        }
        else
        {
            building.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        if (ringIn)
        {
            ringIn.SetActive(!v.RingNearBend);
        }

        if (ringOut)
        {
            ringOut.SetActive(v.RingNearBend);
        }
    }

    ValveBuildingVisual? ReadVisual()
    {
        var buildingOnUp = false;
        var buildingOnDown = false;
        var upIsOutput = false;
        var downIsOutput = false;
        foreach (var portSpec in spec.Ports)
        {
            var state = DirectedPipeOrient.State(portSpec, blockObject.FlipMode.IsFlipped);
            var give = (state & PipePortState.OpenOut) != 0;
            foreach (var d in portSpec.Directions)
            {
                if (d == Direction3D.Up)
                {
                    upIsOutput = give;
                }
                else if (d == Direction3D.Down)
                {
                    downIsOutput = give;
                }
                else
                {
                    continue;
                }

                if ((state & (PipePortState.OpenIn | PipePortState.OpenOut)) == 0)
                {
                    continue;
                }

                var cell = blockObject.TransformCoordinates(portSpec.Coordinates);
                var outward = blockObject.TransformDirection(d);
                if (service.FacesVisualBuilding(pipe, cell, outward, give))
                {
                    if (d == Direction3D.Up)
                    {
                        buildingOnUp = true;
                    }
                    else
                    {
                        buildingOnDown = true;
                    }
                }
            }
        }

        return ValvePipeIo.BuildingVisual(buildingOnUp, buildingOnDown, upIsOutput, downIsOutput);
    }

    static GameObject? NamedChild(Transform root, string name)
    {
        foreach (var child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
            {
                return child.gameObject;
            }
        }

        return null;
    }
}
