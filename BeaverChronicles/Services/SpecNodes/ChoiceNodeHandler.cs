namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class ChoiceNodeHandler(
    ChronicleEventUIHelper uiHelper,
    ILoc t
) : ISpecNodeHandler
{
    public const string NodeType = "Choice";
    public string ForType => NodeType;

    public async void HandleNode(ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var data = node.GetData<ChoiceData>();

        var text = controller.FormatText(t.T(data.TextLoc));

        var ev = controller.Event;
        var evId = ev.Id;
        var topImg = data.TopImagePath ?? (data.TopImage ? ChronicleEventUIHelper.GetTopImagePath(evId) : null);
        var sideImg = data.SideImagePath ?? (data.SideImage ? ChronicleEventUIHelper.GetSideImagePath(evId) : null);

        var page = data.ReuseRecordPage ? controller.CurrentPage : controller.CurrentRecord.AddPage(topImg, sideImg);
        page.AddContent(text);

        SimpleChoiceData[] choices;

        var defaultOpt = 0;
        if (data.Options.Length == 0) // A simple dialog with just an "I see" button, no choices
        {
            choices = [new(t.T("LV.BCEv.GenericOk"))];
        }
        else
        {
            choices = new SimpleChoiceData[data.Options.Length];

            for (int i = 0; i < data.Options.Length; i++)
            {
                var o = data.Options[i];
                var note = o.NoteLoc is not null ? controller.FormatText(t.T(o.NoteLoc)) : null;

                var disabled = o.EnabledConditionNodeId is not null && !controller.EvaluateConditionNode(o.EnabledConditionNodeId);
                if (disabled && o.DisabledTextLoc is not null)
                {
                    note = (note ?? "") + controller.FormatText(t.T(o.DisabledTextLoc));
                }

                choices[i] = new(
                    controller.FormatText(t.T(o.TextLoc)),
                    note,
                    disabled
                );
            }

            if (data.DefaultOption < 1 || data.DefaultOption > choices.Length)
            {
                throw new InvalidOperationException($"DefaultOption {data.DefaultOption} is out of range for event {evId} node {node.Id}");
            }

            defaultOpt = data.DefaultOption - 1;
        }

        var choiceIndex = await uiHelper.ShowChoiceDialogAsync(ev, b =>
        {
            b.SetTextContent(text);

            if (topImg is not null)
            {
                b.SetTopImage(topImg);
            }

            if (sideImg is not null)
            {
                b.SetSideImage(sideImg);
            }

            b.SetDefaultChoice(defaultOpt);
            b.AddChoices(choices);
        });

        if (data.Options.Length == 0)
        {
            controller.TriggerNode(node.NextNodeId);
        }
        else
        {
            var choice = choices[choiceIndex];
            choice.Record(page);

            var record = controller.CurrentRecord;
            var choiceNo = record.RecordNextChoice(choiceIndex) + 1;
            controller.HelperCollection.Flags.MarkEventChoice(
                evId,
                controller.ActiveContext.Occurrence,
                choiceNo,
                choiceIndex + 1);

            var nextNodeId = data.Options[choiceIndex].NextNodeId;
            node.LogVerbose(() => $"Choice made: {choiceIndex + 1}. Going to {nextNodeId}");

            controller.TriggerNode(nextNodeId);
        }

    }

    public void RestoreGameState(ChronicleEventNodeSpec node, SpecChronicleEventController controller)
        => throw NodeHandlerBase.ThrowRestoreGameStateException(NodeType);

}
