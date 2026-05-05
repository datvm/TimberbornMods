namespace TimberUi.Helpers;

public static class ListExtensions
{

    extension<T>(IReadOnlyList<T> list)
    {

        public T? Randomize() => list.Count switch
        {
            0 => default,
            1 => list[0],
            _ => list[UnityEngine.Random.RandomRangeInt(0, list.Count)]
        };

    }

    extension<T>(IEnumerable<T> e)
    {

        public int FindIndex(Predicate<T> predicate)
        {
            var i = 0;
         
            foreach (var item in e)
            {
                if (predicate(item))
                {
                    return i;
                }

                i++;
            }

            return -1;
        }

    }

}
