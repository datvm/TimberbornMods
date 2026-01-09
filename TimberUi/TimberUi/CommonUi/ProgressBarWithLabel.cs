namespace TimberUi.CommonUi;

public readonly record struct ProgressBarWithLabel(TProgressBar ProgressBar, Label Label)
{

    public ProgressBarWithLabel SetProgress(float progress, string? text = default)
    {
        ProgressBar.SetProgress(progress, Label, text);
        return this;
    }

}
