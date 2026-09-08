namespace BuildingHP.Components;

public class BuildingHPWarningComponent : BaseComponent
{

#nullable disable
    MSettings s;
    ILoc t;

    StatusToggle lowHpStatus;
#nullable enable

    BuildingHPComponent? buildingHP;

    [Inject]
    public void Inject(MSettings s, ILoc t)
    {
        this.s = s;
        this.t = t;

        s.ModSettingChanged += (_, _) => RefreshWarning();
    }

    public void Awake()
    {
        buildingHP = this.GetHPComponent();
        buildingHP.OnBuildingHPChanged += _ => RefreshWarning();

        var subject = GetComponentFast<StatusSubject>();
        lowHpStatus = StatusToggle.CreatePriorityStatusWithAlertAndFloatingIcon(
            "buildinghp-low-hp",
            t.T("LV.BHP.LowHpWarningDesc"),
            t.T("LV.BHP.LowHpWarningTitle")
        );
        subject.RegisterStatus(lowHpStatus);
    }

    public void Start()
    {
        RefreshWarning();
    }

    private void RefreshWarning()
    {
        if (!buildingHP) { return; }

        lowHpStatus.Toggle(ShouldShowWarning());
    }

    bool ShouldShowWarning()
    {
        if (s.EnableWarnLowHp.Value)
        {
            var hp = buildingHP!.HP;
            if (hp < s.LowHpThreshold.Value && hp < buildingHP.Durability)
            {
                return true;
            }
        }

        if (s.EnableWarnLowHpPerc.Value)
        {
            if (buildingHP!.HPPercentInt < s.LowHpPercThreshold.Value)
            {
                return true;
            }
        }

        return false;
    }

}
