﻿namespace ScientificProjects.UI;

public class ProjectRow : VisualElement
{
    const int IconSize = 32;
    
    public ScientificProjectInfo ScientificProjectInfo { get; private set; } = null!;
    public ProjectGroupRow ProjectGroupRow { get; private set; } = null!;
    public ProjectRowInfo ProjectRowInfo { get; private set; } = null!;

    public event Action<ScientificProjectInfo, ProjectRow> OnUnlockRequested = delegate { };

    public ProjectRow SetInfo(ScientificProjectInfo p, Texture2D defaultIcon, ILoc t, ProjectGroupRow projectGroupRow)
    {
        ProjectGroupRow = projectGroupRow;
        ScientificProjectInfo = p;
        var spec = p.Spec;

        this.SetAsRow().SetMarginBottom();
        style.alignItems = Align.Center;

        var img = this.AddImage(spec.Icon ?? defaultIcon)
            .SetSize(IconSize, IconSize)
            .SetMarginRight()
            .SetFlexShrink();
        img.style.alignSelf = Align.FlexStart;

        ProjectRowInfo = this.AddChild<ProjectRowInfo>(name: "Info")
            .SetInfo(p, defaultIcon, t);

        if (!spec.HasSteps)
        {
            if (p.Unlocked)
            {
                this.AddLabel("LV.SP.Unlocked".T(t))
                    .SetFlexShrink();
            }
            else
            {
                var btn = this.AddChild<ScienceButton>(name: "UnlockSection");
                btn.Cost = p.Spec.ScienceCost;
                btn.RegisterCallback<ClickEvent>(_ => OnUnlockRequested(p, this));
            }
        }

        return this;
    }

    public bool SetFilter(in ScientificProjectFilter filter)
    {
        var match = ScientificProjectInfo.MatchFilter(filter);
        this.ToggleDisplayStyle(match);

        return match;
    }

}
