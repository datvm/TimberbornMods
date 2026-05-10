namespace BeaverChronicles.Events.Implementations;

public class StartBonus(ChronicleEventUIHelper uiHelper, CharacterSpawnHelper charHelper, GoodsHelper givingHelper, ILoc t) : ChronicleEventBase
{
    static readonly int[] startBonus = [20, 20, 20, 1];

    public override IReadOnlyCollection<EventTriggerSource> TriggerSources => [EventTriggerSource.NewDay];

    public override int GetTriggerWeight(IEventTriggerParameters parameters) => int.MaxValue;

    public override async void OnTriggered(IEventTriggerParameters parameters)
    {
        var page = historyRecord!.AddPage(top: true);

        var content = t.T(ChronicleEventUIHelper.GetDefaultContentLoc(Id));
        page.AddContent(content); 

        var canGiveGood = givingHelper.CanGiveToDistrictCenter(out var dc);
        var canSpawn = charHelper.FindAnySpawnSpot(out var spawnLoc);

        var choices = new SimpleChoiceData[4];
        for (int i = 0; i < 4; i++)
        {
            var note = t.TEventChoiceNote(Id, i).Format(startBonus[i]);
            var disabled = false;

            if (i < 3)
            {
                if (!canGiveGood)
                {
                    disabled = true;
                    note += t.TNoDc();
                }
            }
            else
            {
                if (!canSpawn)
                {
                    disabled = true;
                    note += t.TNoDc();
                }
            }

            choices[i] = new(t.TEventChoice(Id, i), note, disabled);
        }

        var index = await uiHelper.ShowEventDialogAsync(this, b => b
            .SetTextContent(content)
            .SetTopImage()
            .AddChoices(choices)
        );
        var choice = choices[index];
        page.AddContent(choice.Text).AddContent(choice.Note!);

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
