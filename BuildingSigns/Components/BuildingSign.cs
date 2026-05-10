namespace BuildingSigns.Components;

public class BuildingSign(Transform parent, BlockObject blockObject, BuildingSignSpec spec) : IDisposable
{
    static readonly Shader BgShader = Shader.Find("Unlit/Color");
    static readonly Shader FgShader = Shader.Find("Unlit/Transparent");

    const float PixelsPerUnit = 256f;
    const float Padding = .05f;

    public BuildingSignSpec Spec => spec;

    GameObject? signContainer;
    SignObjects signObjects;

    bool initialized;

    public void EnsureInitialized()
    {
        if (initialized) { return; }

        signContainer = new GameObject($"BuildingSign-{spec.DisplayNameLoc}");
        signContainer.transform.SetParent(parent);
        PositionContainer(blockObject._blockObjectSpec);

        signObjects = CreateSignDisplay(signContainer);
        signObjects.SetSize(spec.Width, spec.Height);

        initialized = true;
    }

    public void SetColor(Color color)
    {
        signObjects.
    }

    void PositionContainer(BlockObjectSpec blockObjectSpec)
    {
        var (x, y, z) = blockObjectSpec.Size;

        var face = Spec.Face;
        var dx = Spec.DeltaX;
        var dy = Spec.DeltaY;
        var pivotX = Spec.HorizontalPosition;
        var pivotY = Spec.VerticalPosition;

        const float SurfaceOffset = 0.002f;

        // Position is the selected point on the chosen face.
        // Rotation makes the quad's local +Z point out of the building.
        Vector3 position;
        Quaternion rotation;

        switch (face)
        {
            case Direction3D.Down:
                {
                    // Face at y = 0, facing -Y.
                    var px = ResolvePivot(pivotX, x, dx);
                    var pz = ResolvePivot(pivotY, z, dy);

                    position = new Vector3(px, -SurfaceOffset, pz);
                    rotation = FaceRotation(
                        outward: new Vector3(0f, -1f, 0f),
                        up: new Vector3(0f, 0f, 1f)
                    );
                    break;
                }

            case Direction3D.Up:
                {
                    // Face at y = y, facing +Y.
                    var px = ResolvePivot(pivotX, x, dx);
                    var pz = ResolvePivot(pivotY, z, dy);

                    position = new Vector3(px, y + SurfaceOffset, pz);
                    rotation = FaceRotation(
                        outward: new Vector3(0f, 1f, 0f),
                        up: new Vector3(0f, 0f, 1f)
                    );
                    break;
                }

            case Direction3D.Left:
                {
                    // Face at x = 0, facing -X.
                    var py = ResolvePivot(pivotX, y, dx);
                    var pz = ResolvePivot(pivotY, z, dy);

                    position = new Vector3(-SurfaceOffset, py, pz);
                    rotation = FaceRotation(
                        outward: new Vector3(-1f, 0f, 0f),
                        up: new Vector3(0f, 0f, 1f)
                    );
                    break;
                }

            case Direction3D.Right:
                {
                    // Face at x = x, facing +X.
                    var py = ResolvePivot(pivotX, y, dx);
                    var pz = ResolvePivot(pivotY, z, dy);

                    position = new Vector3(x + SurfaceOffset, py, pz);
                    rotation = FaceRotation(
                        outward: new Vector3(1f, 0f, 0f),
                        up: new Vector3(0f, 0f, 1f)
                    );
                    break;
                }

            case Direction3D.Bottom:
                {
                    // Face at z = 0, facing -Z.
                    var px = ResolvePivot(pivotX, x, dx);
                    var py = ResolvePivot(pivotY, y, dy);

                    position = new Vector3(px, py, -SurfaceOffset);
                    rotation = FaceRotation(
                        outward: new Vector3(0f, 0f, -1f),
                        up: new Vector3(0f, 1f, 0f)
                    );
                    break;
                }

            case Direction3D.Top:
                {
                    // Face at z = z, facing +Z.
                    var px = ResolvePivot(pivotX, x, dx);
                    var py = ResolvePivot(pivotY, y, dy);

                    position = new Vector3(px, py, z + SurfaceOffset);
                    rotation = FaceRotation(
                        outward: new Vector3(0f, 0f, 1f),
                        up: new Vector3(0f, 1f, 0f)
                    );
                    break;
                }

            default:
                throw new ArgumentOutOfRangeException(nameof(Spec.Face), face, null);
        }

        signContainer!.transform.SetLocalPositionAndRotation(position, rotation);
    }

    static SignObjects CreateSignDisplay(GameObject root)
    {
        // Background
        var bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Object.Destroy(bg.GetComponent<Collider>());
        bg.name = "Background";
        bg.transform.SetParent(root.transform, false);
        bg.transform.localPosition = new Vector3(0f, 0f, 0.001f);

        var bgRenderer = bg.GetComponent<MeshRenderer>();
        bgRenderer.material = new Material(BgShader)
        {
            color = Color.black,
        };

        // Foreground texture
        var fg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Object.Destroy(fg.GetComponent<Collider>());
        fg.name = "Texture";
        fg.transform.SetParent(root.transform, false);
        fg.transform.localPosition = new Vector3(0f, 0f, -0.001f);
        var fgRenderer = fg.GetComponent<MeshRenderer>();
        fgRenderer.material = new Material(FgShader);

        fg.GetComponent<MeshRenderer>();
        return new(bg, bgRenderer.material, fg, fgRenderer.material);
    }

    public void Dispose()
    {
        if (!initialized) { return; }

        Object.Destroy(signObjects.BackgroundQuad);
        Object.Destroy(signObjects.BackgroundMaterial);
        Object.Destroy(signObjects.ForegroundQuad);
        Object.Destroy(signObjects.ForegroundMaterial);
        Object.Destroy(signContainer);
    }

    static float ResolvePivot(object pivot, float size, float delta)
    {
        return pivot.ToString() switch
        {
            "Left" or "Bottom" or "Start" => delta,
            "Center" or "Middle" => size * 0.5f + delta,
            "Right" or "Top" or "End" => size - delta,
            _ => throw new ArgumentOutOfRangeException(nameof(pivot), pivot, null)
        };
    }

    static Quaternion FaceRotation(Vector3 outward, Vector3 up)
    {
        // Quad local +Z points outward from the building face.
        // Unity stores Transform local rotation as a Quaternion, and Quaternion.Euler/LookRotation are the normal way
        // to construct those rotations in scripts.
        return Quaternion.LookRotation(outward, up);
    }

    readonly record struct SignObjects(GameObject BackgroundQuad, Material BackgroundMaterial, GameObject ForegroundQuad, Material ForegroundMaterial)
    {
        public void SetSize(float w, float h)
        {
            BackgroundQuad.transform.localScale = ForegroundQuad.transform.localScale
                = new Vector3(w + 2 * Padding, h + 2 * Padding, 1);
        }
    }
}
