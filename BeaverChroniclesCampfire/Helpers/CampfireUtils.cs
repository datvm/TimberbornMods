namespace BeaverChroniclesCampfire.Helpers;

public static class CampfireUtils
{
    public const int NightHour = 20;
    public const float NightTime = NightHour / 24f;

    public const string TopImagePath = ChronicleEventUIHelper.RecommendedImagePath + "CampfireMystery1_Top";
    public const string SideImagePath = ChronicleEventUIHelper.RecommendedImagePath + "CampfireMystery1_Side";

    public const string Chapter1Id = nameof(CampfireMystery1);
    public const string Chapter2Id = nameof(CampfireMystery2);
    public const string Chapter3Id = nameof(CampfireMystery3);
    public const string Chapter4Id = nameof(CampfireMystery4);
    static readonly ImmutableArray<string> ChapterIds = [Chapter1Id, Chapter2Id, Chapter3Id, Chapter4Id];

    extension(ChronicleEventService service)
    {
        public int GetCampfireTriggerWeight(int index)
            => index == 0 || service.HasCompletedEvent(ChapterIds[index - 1]) ? 100 : 0;

        public EventHistoryRecord GetCampfireRecord(int index)
            => service.History.Get(ChapterIds[index]).First();

    }

    extension(ActiveChronicleEventService activeService)
    {

        public void WaitUntilNighttime(Action onTimeLimit, IDayNightCycle dayNightCycle, ILoc t)
        {
            activeService.ClearTimeLimit();

            if (dayNightCycle.IsNightTime())
            {
                onTimeLimit();
                return;
            }

            activeService.SetActiveDescription(t.T("LV.BCEv.CampfireMystery.WaitNight"));
            activeService.RegisterTimeLimit(dayNightCycle.GetDaysUntilNightTime(), onTimeLimit);
        }


    }

    extension(IDayNightCycle dayNightCycle)
    {
        public float GetDaysUntilNightTime()
        {
            var time = dayNightCycle.DayProgress;
            return time > NightTime ? 0 : NightTime - time;
        }

        public bool IsNightTime() => dayNightCycle.DayProgress >= NightTime;
    }

    extension(EventHistoryRecord record)
    {

        public float TimeUntilNight
        {
            get => float.Parse(record.CustomParameters.GetOrDefault("TimeUntilNight") ?? "0");
            set => record.CustomParameters["TimeUntilNight"] = value.ToString();
        }

        public EventHistoryPage AddCampfirePage(bool top = false, bool side = false) => record.AddPage(
            top ? TopImagePath : null,
            side ? SideImagePath : null);

    }

    extension(ChronicleEventChoiceDialogBuilder diag)
    {

        public ChronicleEventChoiceDialogBuilder SetCampfireSideImage() => diag.SetSideImage(SideImagePath);
        public ChronicleEventChoiceDialogBuilder SetCampfireTopImage() => diag.SetTopImage(TopImagePath);

    }

}
