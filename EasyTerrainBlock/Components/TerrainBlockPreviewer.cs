namespace EasyTerrainBlock.Components;

[AddTemplateModule2(typeof(GroundRaiserSpec))]
public class TerrainBlockPreviewer(GroundRaisingSupportService service) : BaseComponent, IAwakableComponent, IPreviewSelectionListener
{

    BlockObject blockObject = null!;

    public void Awake()
    {
        blockObject = GetComponent<BlockObject>();
    }

    public void OnPreviewSelect() => service.HighlightDestroyingObjectsAt(blockObject.Coordinates);
    public void OnPreviewUnselect() => service.UnhighlightDestruction();
}
