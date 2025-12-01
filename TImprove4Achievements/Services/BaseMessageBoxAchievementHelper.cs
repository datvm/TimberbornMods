namespace TImprove4Achievements.Services;

public abstract class BaseMessageBoxAchievementHelper<T>(string baseLocKey, int stepsCount, DialogService diag, ILoc t)
    : BaseAchievementHelper<T>(baseLocKey, stepsCount) where T : Achievement
{

    protected readonly string baseLocKey = baseLocKey;
    protected readonly DialogService diag = diag;
    protected readonly ILoc t = t;

    protected abstract object[] GetParameters(int step);

    public override void ActivateStep(int step) => diag.Alert(GetMessage(step));
    protected virtual string GetMessage(int step) => string.Format(t.T($"{baseLocKey}.Msg{step}"), GetParameters(step));

}
