namespace BlueprintRelics.UI.RewardPicker;

public interface IRewardPanel
{
    IRelicReward Reward { get; }

    event EventHandler OnSelected;

    void SetReward(IRelicReward reward);
    void Unselect();
}

public abstract class RewardPanelBase<T>(ILoc t) : VisualElement, IRewardPanel where T : IRelicReward
{
    static readonly Color UnselectColor = Color.gray;
    static readonly Color SelectColor = Color.yellowGreen;

    protected readonly ILoc t = t;

    IRelicReward IRewardPanel.Reward => Reward;
    public T Reward { get; private set; } = default!;
    public event EventHandler OnSelected = null!;

    public void SetReward(IRelicReward reward)
    {
        Reward = (T)reward;

        this.SetBorder(UnselectColor, 2)
            .SetPadding(10)
            .SetMargin(marginY: 10);

        this.AddLabelHeader(t.T(reward.TitleLoc).Bold().Color(TimberbornTextColor.Yellow)).SetMarginBottom();
        RegisterCallback<ClickEvent>(_ => Select());

        InitializeUI();
    }

    public void Select()
    {
        this.SetBorder(SelectColor);
        OnSelected(this, EventArgs.Empty);
    }

    public void Unselect()
    {
        this.SetBorder(UnselectColor);
    }

    protected abstract void InitializeUI();

}
