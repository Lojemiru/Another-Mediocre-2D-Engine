namespace AM2E.Graphics;

public struct CullingBounds
{
    public readonly int L;
    public readonly int U;
    public readonly int R;
    public readonly int D;

    public CullingBounds(int l, int u, int r, int d)
    {
        L = l;
        U = u;
        R = r;
        D = d;
    }
}