namespace MapTransformer.Transform;

enum MapOp
{
    Resize,
    AddHeight,
    RemoveHeight,
    Rotate,
    Flip,
}

enum EnlargeFill
{
    Empty,
    CopyEdge,
    Mirror,
}

enum MapPivot
{
    MinMin,
    MaxMin,
    MinMax,
    MaxMax,
    Center,
}
