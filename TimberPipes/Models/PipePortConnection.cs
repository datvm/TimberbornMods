namespace TimberPipes.Models;

public readonly record struct PipePortConnection(PipePort A, PipePort B)
{
    public PipePort GetOther(PipePort curr)
    {
        if (curr == A)
        {
            return B;
        }
        else if (curr == B)
        {
            return A;
        }

        throw new ArgumentException("The provided port is not part of this connection.", nameof(curr));
    }
}
