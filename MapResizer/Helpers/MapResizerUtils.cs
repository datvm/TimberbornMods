namespace MapResizer.Helpers;

public static class MapResizerUtils
{

    public static int GetEnlargeCoord(int oldSize, int x, EnlargeStrategy strat)
    {
        if (oldSize == 1) { return 0; }

        if (x < oldSize) { return x; }

        switch (strat)
        {
            case EnlargeStrategy.Mirror:
                {
                    int period = 2 * oldSize;
                    int mod = x % period;
                    return (mod < oldSize) ? mod : period - mod - 1;
                }

            default:
                return oldSize - 1;
        }
    }

}
