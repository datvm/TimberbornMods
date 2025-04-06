using ScientificProjects.Extensions;

namespace ScientificProjects.UI;

partial class ScientificProjectDialog
{
    public bool ForceUnlock => input.IsKeyHeld(BuildingToolLocker.InstantUnlockKey);

    void RequestUnlock(ScientificProjectInfo p)
    {
        var forceUnlock = ForceUnlock;
        if (forceUnlock)
        {
            OnConfirmedUnlock(p, forceUnlock);
        }
        else
        {
            var locked = projects.CanUnlock(p.Spec.Id);
            if (locked is null)
            {
                diagShower.Create()
                    .SetMessage("LV.SP.UnlockConfirm".T(t, p.Spec.DisplayName, p.Spec.ScienceCost))
                    .SetConfirmButton(() => OnConfirmedUnlock(p, forceUnlock), "Core.OK".T(t))
                    .SetCancelButton(DoNothing, "Core.Cancel".T(t))
                    .Show();
            }
            else
            {
                ShowUnlockError(locked);
                return;
            }
        }
    }

    void OnConfirmedUnlock(ScientificProjectInfo p, bool forceUnlock)
    {
        var error = projects.TryToUnlock(p.Spec.Id, forceUnlock);
        if (error is not null)
        {
            ShowUnlockError(error);
            return;
        }

        RefreshContent();

        if (p.Spec.NeedReload)
        {
            diagShower.Create()
                .SetLocalizedMessage("LV.SP.ReloadNotice")
                .Show();
        }
    }

    void ShowUnlockError(string error)
    {
        diagShower.Create()
            .SetMessage(error)
            .SetConfirmButton(DoNothing, "Core.OK".T(t))
            .Show();
    }

}
