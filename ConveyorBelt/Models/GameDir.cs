namespace ConveyorBelt.Models;

static class GameDir
{
    public static Direction3D Game(this BeltDir dir) => (Direction3D)(int)dir;

    public static BeltDir Sim(this Direction3D dir) => (BeltDir)(int)dir;
}
