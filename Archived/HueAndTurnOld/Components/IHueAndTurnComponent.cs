namespace HueAndTurn.Components;

public interface IHueAndTurnComponent
{

    bool CanHaveTransparency { get; }
    string DisplayName { get; }
    bool HasFluid { get; }
    string PrefabName { get; }
    PrefabSpec? PrefabSpec { get; }
    ReadOnlyHueAndTurnProperties Properties { get; }
    bool RotationPivotSupported { get; }

    void ApplyProperties(ReadOnlyHueAndTurnProperties properties);
    void Reset();
    void SetColor(Color? color);
    void SetFluidColor(Color? color);
    void SetRotation(int? rotation);
    void SetRotationPivot(Vector2Int? rotationPivot);
    void SetRotationWithPivot(int? rotation, Vector2Int? rotationPivot);
    void SetRotationXZ(Vector2Int? rotationXZ);
    void SetScale(Vector3Int? scale);
    void SetTranslation(Vector3Int? translation);
    void SetTransparency(int? transparency);
}
