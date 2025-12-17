
namespace ConfigurableToolGroups.UI.BuiltInRootProviders;

public class MapEditorToolButtonsCustomRootElement(MapEditorToolButtons button, ToolButtonService toolButtonService)
    : BuiltInButtonCustomRootElement<MapEditorToolButtons>(button)
{

    public override IEnumerable<IToolHotkeyDefinition> GetHotkeys()
    {
        foreach (var btn in toolButtonService.ToolButtons)
        {
            switch (btn.Tool)
            {
                case AbsoluteTerrainHeightBrushTool:
                    yield return Create(btn, AbsoluteTerrainHeightBrushTool.TitleLocKey);
                    break;
                case RelativeTerrainHeightBrushTool:
                    yield return Create(btn, RelativeTerrainHeightBrushTool.TitleLocKey);
                    break;
                case SculptingTerrainBrushTool:
                    yield return Create(btn, SculptingTerrainBrushTool.TitleLocKey);
                    break;
                case NaturalResourceSpawningBrushTool:
                    yield return Create(btn, NaturalResourceSpawningBrushTool.TitleLocKey);
                    break;
                case NaturalResourceRemovalBrushTool:
                    yield return Create(btn, NaturalResourceRemovalBrushTool.TitleLocKey);
                    break;
                case EntityBlockObjectDeletionTool:
                    yield return Create(btn, EntityBlockObjectDeletionTool.ToolTitleLocKey);
                    break;
                case ThumbnailCapturingTool:
                    yield return Create(btn, ThumbnailCapturingTool.TitleLocKey);
                    break;
                case MapMetadataTool:
                    yield return Create(btn, MapMetadataTool.TitleLocKey);
                    break;
            }
        }

        static ButtonToolHotkeyDefinition Create(ToolButton btn, string locKey) => new(locKey, locKey, btn);
    }

}
