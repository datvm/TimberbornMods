namespace ScientificProjects.UI;

partial class ScientificProjectDialog
{
    public bool ForceUnlock => input.IsKeyHeld(BuildingToolLocker.InstantUnlockKey);

    void RequestUnlock(ScientificProjectInfo p)
    {
        var status = projects.TryToUnlock(p.Spec.Id, ForceUnlock);
        if (status is not null)
        {
            ShowUnlockError(status.Value);
            return;
        }

        RefreshContent();
    }

    void ShowUnlockError(ScientificProjectUnlockStatus error)
    {
        diagShower.Create()
            .SetMessage($"LV.SP.UnlockErr{error}".T(t))
            .SetConfirmButton(DoNothing, "Core.OK".T(t))
            .Show();
    }

}
