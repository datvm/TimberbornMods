namespace BeaverChronicles.Events.Implementations;

public class StartBonus(ChronicleEventUIHelper uiHelper, CharacterSpawnHelper charHelper, GoodsHelper givingHelper, ILoc t) : ChronicleEventBase
{
    static readonly int[] startBonus = [20, 20, 20, 1];

    public override IReadOnlyCollection<EventTriggerSource> TriggerSources => [EventTriggerSource.NewDay];

    public override int GetTriggerWeight(IEventTriggerParameters parameters, ChronicleEventService chronicleEventService) => int.MaxValue;

    protected override async void OnTriggered(IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        var page = record.AddPage(top: true);

        var content = t.T(ChronicleEventUIHelper.GetDefaultContentLoc(Id));
        page.AddContent(content); 

        var canGiveGood = givingHelper.CanGiveToDistrictCenter(out var dc);
        var canSpawn = charHelper.FindAnySpawnSpot(out var spawnLoc);

        var choices = SimpleChoiceData.Create(4, Id, t, (i, n) =>
        {
            n = n.Format(startBonus[i]);

            if (i < 3)
            {
                if (!canGiveGood)
                {
                    return n + t.TNoDc();
                }
            }
            else
            {
                if (!canSpawn)
                {
                    return n + t.TNoDc();
                }
            }

            return n;
        }, i => i < 3 ? !canGiveGood : !canSpawn);


        var index = await uiHelper.ShowChoiceDialogAsync(this, b => b
            .SetTextContent(content)
            .SetTopImage()
            .AddChoices(choices)
        );
        choices[index].Record(page);

        var amount = startBonus[index];

        if (index == 3)
        {
            if (charHelper.FindAnySpawnSpot(out var loc))
            {
                charHelper.Spawn(amount, loc, CharacterType.ChildBeaver);
            }
        }
        else if (index >= 0)
        {
            givingHelper.GiveToDistrictCenter([new(index switch {
                0 => "Berries",
                1 => "Water",
                2 => "Log",
                _ => throw new ArgumentOutOfRangeException(),
            }, amount)], dc);
        }

        Conclude();
    }

}
