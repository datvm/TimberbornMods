namespace DirectionalDynamite.Components;

public interface IDirectionalDynamiteComponent
{
    Vector3Int Coordinates { get; }
    Direction3D Direction { get; set; }
    bool DoNotTriggerNeighbor { get; set; }
    int MaxDepth { get; }

    void HideIndicator();
    void ShowIndicator(Sprite arrow);
}
