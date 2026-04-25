namespace RadialToolbar.Models;

public static class RadialToolbarExtensions
{

    public static ImmutableArray<Direction> AllDirections = [ Direction.Up, Direction.Down, Direction.Left, Direction.Right ];

    extension(Direction r)
    {

        public (int deltaX, int deltaY) GetDirectionDeltas()
        {
            int deltaX = 0, deltaY = 0;
            if ((r & Direction.Left) != 0)
            {
                deltaX = -1;
            }
            else if ((r & Direction.Right) != 0)
            {
                deltaX = 1;
            }
            if ((r & Direction.Up) != 0)
            {
                deltaY = -1;
            }
            else if ((r & Direction.Down) != 0)
            {
                deltaY = 1;
            }
            return (deltaX, deltaY);
        }

        public string DescribeHotkey(KeyBindingDescriber describer)
        {
            var id = "";

            foreach (var d in AllDirections)
            {
                if ((r & d) != 0)
                {
                    id += d.ToString();
                }
            }

            return describer.GetOrDefault("RadialNav" + id);
        }

    }
    
    extension(KeyBindingDescriber describer)
    {
        public string GetOrDefault(string id)
            => describer.TryGetKeyBindingText(id, out var result) ? result : "[?]";
    }

}
