
namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class ChoiceNodeHandler(
    ChronicleEventUIHelper uiHelper,
    ILoc t
) : ISpecNodeHandler
{
    public const string NodeType = "Choice";
    public string ForType => NodeType;

    public async void HandleNode(ChronicleEventNodeSpec node, SpecChronicleEventHelper helper)
    {
        var data = node.GetData<ChoiceData>();

        var text = helper.FormatText(t.T(data.TextLoc));

        var ev = helper.Event;
        var evId = ev.Id;
        var topImg = data.TopImagePath ?? (data.TopImage ? ChronicleEventUIHelper.GetTopImagePath(evId) : null);
        var sideImg = data.SideImagePath ?? (data.SideImage ? ChronicleEventUIHelper.GetSideImagePath(evId) : null);

        var page = data.ReuseRecordPage ? helper.CurrentPage : helper.CurrentRecord.AddPage(topImg, sideImg);
        page.AddContent(text);

        var choices = new SimpleChoiceData[data.Options.Length];
        for (int i = 0; i < data.Options.Length; i++)
        {
            var o = data.Options[i];
            var note = o.NoteLoc is not null ? helper.FormatText(t.T(o.NoteLoc)) : null;
            var disabled = o.EnabledConditionNodeId is not null && !helper.EvaluateConditionNode(o.EnabledConditionNodeId);

            choices[i] = new(
                helper.FormatText(t.T(o.TextLoc)),
                note,
                disabled
            );
        }

        if (data.DefaultOption < 1 || data.DefaultOption > choices.Length)
        {
            throw new InvalidOperationException($"DefaultOption {data.DefaultOption} is out of range for event {evId} node {node.Id}");
        }

        var choiceIndex = await uiHelper.ShowChoiceDialogAsync(ev, b => {
            b.SetTextContent(text);

            if (topImg is not null)
            {
                b.SetTopImage(topImg);
            }

            if (sideImg is not null)
            {
                b.SetSideImage(sideImg);
            }

            b.SetDefaultChoice(data.DefaultOption - 1);
            b.AddChoices(choices);
        });
        choices[choiceIndex].Record(page);

        helper.TriggerNode(data.Options[choiceIndex].NextNodeId);
    }

    public void RestoreGameState(ChronicleEventNodeSpec node, SpecChronicleEventHelper helper)
        => HandleNode(node, helper); // Should not happen, there should be no game save at this node.

}
