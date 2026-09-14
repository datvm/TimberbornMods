namespace TimberPipes.Models;

public sealed class PipeFlowScratch
{
    public float[] Heads { get; private set; } = [];
    public float[] Desired { get; private set; } = [];
    public float[] Outgoing { get; private set; } = [];
    public float[] Incoming { get; private set; } = [];
    public float[] ScaleOut { get; private set; } = [];
    public float[] ScaleIn { get; private set; } = [];
    public List<int>[] Adj { get; private set; } = [];
    public List<int>[] InAdj { get; private set; } = [];

    public void Ensure(int n, int edgeCount)
    {
        Heads = Grow(Heads, n);
        Desired = Grow(Desired, edgeCount);
        Outgoing = Grow(Outgoing, n);
        Incoming = Grow(Incoming, n);
        ScaleOut = Grow(ScaleOut, n);
        ScaleIn = Grow(ScaleIn, n);
        EnsureAdj(n);
    }

    public void BuildOutflows(int n, ReadOnlySpan<PipeFlowEdge> edges)
    {
        for (var i = 0; i < n; i++)
        {
            Adj[i].Clear();
            InAdj[i].Clear();
        }

        for (var e = 0; e < edges.Length; e++)
        {
            var edge = edges[e];
            if (edge.AllowAToB && InRange(edge.A, n) && InRange(edge.B, n))
            {
                Adj[edge.A].Add(edge.B);
                InAdj[edge.B].Add(edge.A);
            }

            if (edge.AllowBToA && InRange(edge.A, n) && InRange(edge.B, n))
            {
                Adj[edge.B].Add(edge.A);
                InAdj[edge.A].Add(edge.B);
            }
        }
    }

    void EnsureAdj(int n)
    {
        Adj = GrowAdj(Adj, n);
        InAdj = GrowAdj(InAdj, n);
    }

    static List<int>[] GrowAdj(List<int>[] adj, int n)
    {
        if (adj.Length >= n)
        {
            return adj;
        }

        var next = new List<int>[n];
        for (var i = 0; i < adj.Length; i++)
        {
            next[i] = adj[i];
        }

        for (var i = adj.Length; i < n; i++)
        {
            next[i] = [];
        }

        return next;
    }

    static float[] Grow(float[] a, int n)
        => a.Length >= n ? a : new float[n];

    static bool InRange(int i, int n)
        => i >= 0 && i < n;
}
