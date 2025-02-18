namespace SuperCursor.Services.CoordDescribers;

public interface ICoordDescriber
{
    void Describe(StringBuilder builder, in CursorCoordinates block);
}