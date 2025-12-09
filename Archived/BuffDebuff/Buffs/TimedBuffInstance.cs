namespace BuffDebuff;

public abstract class TimedBuffInstance<TBuff> : TimedBuffInstance, IBuffInstance<TBuff>
    where TBuff : IBuff
{
    public new TBuff Buff => (TBuff)base.Buff;
}

public abstract class TimedBuffInstance : BuffInstance
{

    protected abstract IBuffService Buffs { get; set; }
    protected abstract IDayNightCycle DayNight { get; set; }
    protected abstract ILoc T { get; set; }

    public abstract float StartingTime { get; }

    [JsonProperty]
    public virtual float RemainingTime { get; protected set; }

    [JsonProperty]
    public virtual bool AlreadySetStartingTime { get; protected set; }

    protected virtual bool RunWhileInactive => false;

    public override void Init()
    {
        if (!AlreadySetStartingTime)
        {
            AlreadySetStartingTime = true;
            RemainingTime = StartingTime;
        }
    }

    public override string? AdditionalDescription
    {
        get => FormatTime(RemainingTime);
        protected set { }
    }

    public override void Update()
    {
        base.Update();

        if (Active || RunWhileInactive)
        {
            RemainingTime -= DayNight.FixedDeltaTimeInHours;

            if (RemainingTime <= 0)
            {
                TimeUp();
            }
        }
    }

    protected virtual void TimeUp()
    {
        Active = false;
        Buffs.Remove(this);
    }

    string FormatTime(float hours)
    {
        var allDayLength = DayNight.DaytimeLengthInHours + DayNight.NighttimeLengthInHours;
        var days = (int)(hours / allDayLength);

        var dayStr = "";
        if (days > 1)
        {
            dayStr = T.T("Time.DaysShort", days) + " ";
        }

        hours -= days * allDayLength;
        return "⏱︎ " + dayStr + T.T("Time.HoursShort", (int)hours);
    }

    [Inject]
    public void InjectTimedBuff(IBuffService buffs, IDayNightCycle dayNight, ILoc t)
    {
        Buffs = buffs;
        DayNight = dayNight;
        T = t;
    }

    [Obsolete("You no longer need to call this since v7.0.6. The base class call InjectTiSmedBuff for you")]
    protected virtual void Inject(IBuffService buffs, IDayNightCycle dayNight, ILoc t) { }

}
