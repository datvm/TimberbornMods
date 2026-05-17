namespace TimberLive.Services;

public class UnionFind
{
    readonly int[] parent;
    readonly int[] rank;

    public UnionFind(int size)
    {
        parent = new int[size];
        rank = new int[size];
        for (int i = 0; i < size; i++)
        {
            parent[i] = i;
            rank[i] = 0;
        }
    }

    public int Find(int x) => parent[x] == x ? x : parent[x] = Find(parent[x]);

    public void Union(int x, int y)
    {
        var px = Find(x);
        var py = Find(y);
        if (px == py) { return; }

        if (rank[px] < rank[py])
        {
            parent[px] = py;
        }
        else if (rank[px] > rank[py])
        {
            parent[py] = px;
        }
        else
        {
            parent[py] = px;
            rank[px]++;
        }
    }
}
