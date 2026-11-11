namespace ModdableAlternativeBuildingStyle.Components;

[AddTemplateModule2(typeof(InventorWeatherVaneSpec))]
public class InventorWeatherVane(WindService wind, EventBus events) : BaseComponent, IInitializableEntity, IDeletableEntity, IAlternativeBuildingStyleListener
{
    const string StyleName = "Weather Station";
    const string ArrowName = "WeatherVaneWindArrow";

    Transform? arrow;

    public void InitializeEntity()
    {
        events.Register(this);
        if (SelectedStyle() == StyleName)
        {
            CaptureArrow(Transform);
        }
    }

    public void DeleteEntity()
    {
        events.Unregister(this);
    }

    public void OnAlternativeStyleChanged(AlternativeBuildingStyleComponent building, string? style)
    {
        if (style != StyleName)
        {
            arrow = null;
            return;
        }

        CaptureArrow(building.Transform);
    }

    [OnEvent]
    public void OnWindChanged(WindChangedEvent _)
    {
        Align();
    }

    string? SelectedStyle()
    {
        var styles = GetComponent<AlternativeBuildingStyleComponent>();
        if (!styles)
        {
            return null;
        }

        return styles.AlternativeStyle;
    }

    void CaptureArrow(Transform root)
    {
        arrow = FindNamed(root, ArrowName);
        Align();
    }

    void Align()
    {
        if (!arrow)
        {
            return;
        }

        var direction = wind.WindDirection;
        // WindRotationAnimator yaws by SignedAngle(Vector2.down, wind), which aims local +Z along this vector.
        // The arrow head sits on local -Z, the same way #WindLogMisc points.
        var forward = CoordinateSystem.GridToWorld(new Vector3(direction.x, -direction.y, 0f));
        if (forward.sqrMagnitude < 0.0001f)
        {
            return;
        }

        arrow.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    static Transform? FindNamed(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var found = FindNamed(root.GetChild(i), name);
            if (found)
            {
                return found;
            }
        }

        return null;
    }
}
