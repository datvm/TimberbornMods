namespace ScientificProjects.UI;

partial class ScientificProjectDialog
{
    public bool ForceUnlock => input.IsKeyHeld(BuildingToolLocker.InstantUnlockKey);

    void RequestUnlock(ScientificProjectInfo p)
    {
        var status = projects.TryToUnlock(p.Spec.Id, ForceUnlock);
        if (status is not null)
        {
            ShowUnlockError(status.Value, 
                requiredName: status == ScientificProjectUnlockStatus.RequirementLocked 
                    ? p.PreqProject!.Spec.DisplayName
                    : null);
            return;
        }

        RefreshContent();
    }

    void ShowUnlockError(ScientificProjectUnlockStatus error, string? requiredName)
    {
        var msg = $"LV.SP.UnlockErr{error}".T(t);
        if (requiredName is not null)
        {
            msg = string.Format(msg, requiredName);
        }

        diagShower.Create()
            .SetMessage(msg)
            .SetConfirmButton(DoNothing, "Core.OK".T(t))
            .Show();
    }

}
