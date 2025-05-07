namespace TImprove4Mods.Services;

public class ModCompWarningService(
    DialogBoxShower diagShower,
    ModRepository modRepository,
    ILoc t
) : ILoadableSingleton
{

    public void Load()
    {
        _ = Task.Run(ModCompatibilityService.Instance.FetchDataAsync);

        CheckAndShowIssue();
    }

    public async Task CheckForIssueAsync()
    {
        var serv = ModCompatibilityService.Instance;
        await serv.FetchDataAsync();
        serv.LoadData();

        if (!CheckAndShowIssue())
        {
            diagShower.Create()
                .SetMessage("LV.T4Mods.NoIssue".T(t))
                .Show();
        }
    }

    bool CheckAndShowIssue()
    {
        var issue = ModCompatibilityService.Instance.CheckForFirstIssue(modRepository);
        if (issue is null) { return false; }

        ShowIssue(issue.Value);
        return true;
    }

    void ShowIssue(in ModIssue issue)
    {
        var (message, url) = GetMessageForIssue(issue);

        var builder = diagShower.Create()
            .SetMessage(message);

        if (url != null)
        {
            builder
                .SetConfirmButton(() => ShowUrl(url))
                .SetDefaultCancelButton();
        }

        builder.Show();
    }

    void ShowUrl(string url)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    (string message, string? url) GetMessageForIssue(in ModIssue issue)
    {
        string message;
        string? url = null;

        var modName = issue.Mod.DisplayName;

        switch (issue.Issue)
        {
            case ObsoleteMod ob:
                if (ob.Replacement is null || (ob.Replacement.Id is null && ob.Replacement.Name is null))
                {
                    message = "LV.T4Mods.IssueObsolete".T(t, modName);
                }
                else
                {
                    url = ob.Replacement.Url;

                    message = (url is null ? "LV.T4Mods.IssueObsoleteWRep" : "LV.T4Mods.IssueObsoleteWRepUrl")
                        .T(t, modName, ob.Replacement.Name ?? ob.Replacement.Id);
                }

                break;
            case IncompatibleModIssue incomp:
                message = "LV.T4Mods.IssueIncomp".T(t,
                    modName,
                    incomp.IncompatibleMod.DisplayName,
                    $"LV.T4Mods.IncompReason{incomp.Data.Reason}".T(t));

                break;
            default:
                message = "Unknown";
                break;
        }

        var note = issue.Issue.Note;
        if (note != null)
        {
            message += Environment.NewLine + "LV.T4Mods.AuthorNote".T(t, note);
        }

        return (message, url);
    }

}
