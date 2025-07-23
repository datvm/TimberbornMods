namespace BenchmarkAndOptimizer.UI;

public class BenchmarkMenu(
    IOptionsBox optionsBox,
    ILoc t,
    BenchmarkDialog diag,
    SpeedManager speedManager,
    EventBus eb
) : ILoadableSingleton
{

    readonly GameOptionsBox box = (GameOptionsBox)optionsBox;

    public void Load()
    {
        if (MStarter.BenchmarkPatched)
        {
            AddBmButton();
            eb.Register(this);
        }
    }

    [OnEvent]
    public void OnBenchmarkEnd(OnBenchmarkEndEvent e)
    {
        ShowBmDialog();
    }

    void AddBmButton()
    {
        var root = box._root;

        var btnBenchmark = root.AddMenuButton(t.T("LV.BO.Benchmark"), onClick: OnShowBmButtonClicked);
        btnBenchmark.InsertSelfAfter(root.Q("ResumeButton"));
    }

    void OnShowBmButtonClicked()
    {
        box.ResumeClicked(null!);
        ShowBmDialog();
    }

    async void ShowBmDialog()
    {
        if (await diag.ShowAsync())
        {
            if (speedManager._speedBefore == 0)
            {
                speedManager._speedBefore = 1;
            }
            speedManager.UnlockSpeed();

            if (speedManager.CurrentSpeed == 0)
            {
                speedManager.ChangeSpeed(1);
            }
        }
    }

}
